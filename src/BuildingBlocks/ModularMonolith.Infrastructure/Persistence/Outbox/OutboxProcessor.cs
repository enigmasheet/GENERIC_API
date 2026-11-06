using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularMonolith.Infrastructure.Options;
using ModularMonolith.SharedKernel.Abstractions;

namespace ModularMonolith.Infrastructure.Persistence.Outbox;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    IEnumerable<ModuleDbContextRegistration> registrations,
    IOptionsMonitor<OutboxOptions> optionsMonitor,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private DateTimeOffset _lastCleanup = DateTimeOffset.MinValue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var options = optionsMonitor.CurrentValue;

            if (!options.Enabled)
            {
                await DelayAsync(TimeSpan.FromSeconds(5), stoppingToken);
                continue;
            }

            try
            {
                foreach (var registration in registrations)
                {
                    await ProcessContextAsync(registration.ContextType, options, stoppingToken);
                }

                await CleanupIfDueAsync(options, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox processing iteration failed.");
            }

            await DelayAsync(TimeSpan.FromSeconds(options.PollIntervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessContextAsync(
        Type contextType,
        OutboxOptions options,
        CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();

        if (scope.ServiceProvider.GetService(contextType) is not ModuleDbContext dbContext)
        {
            logger.LogError("Outbox context {ContextType} is not registered.", contextType.FullName);
            return;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var sql = $"""
            SELECT * FROM "{dbContext.Schema}"."{ModuleDbContext.OutboxTableName}"
            WHERE "Status" = {(int)OutboxMessageStatus.Pending}
              AND ("NextAttemptAt" IS NULL OR "NextAttemptAt" <= now())
            ORDER BY "OccurredAt"
            LIMIT {options.BatchSize}
            FOR UPDATE SKIP LOCKED
            """;

        var messages = await dbContext.OutboxMessages
            .FromSqlRaw(sql)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        var handlers = scope.ServiceProvider
            .GetServices<IIntegrationEventHandler>()
            .ToLookup(handler => handler.EventType.FullName, StringComparer.Ordinal);

        foreach (var message in messages)
        {
            var matchingHandlers = handlers[message.Type].ToArray();

            if (matchingHandlers.Length == 0)
            {
                message.MarkFailed(
                    DateTimeOffset.UtcNow,
                    $"No integration event handler is registered for '{message.Type}'.",
                    options.MaxAttempts);
                logger.LogWarning(
                    "No handler registered for outbox message {MessageId} of type {MessageType}.",
                    message.Id,
                    message.Type);
                continue;
            }

            try
            {
                var integrationEvent = JsonSerializer.Deserialize(
                    message.Payload,
                    matchingHandlers[0].EventType,
                    OutboxJson.Options);

                if (integrationEvent is null)
                {
                    message.MarkFailed(
                        DateTimeOffset.UtcNow,
                        "Integration event payload could not be deserialized.",
                        options.MaxAttempts);
                    continue;
                }

                foreach (var handler in matchingHandlers)
                {
                    await handler.HandleAsync(integrationEvent, cancellationToken);
                }

                message.MarkProcessed(DateTimeOffset.UtcNow);
            }
            catch (Exception exception)
            {
                message.MarkFailed(DateTimeOffset.UtcNow, exception.Message, options.MaxAttempts);
                logger.LogError(
                    exception,
                    "Outbox message {MessageId} of type {MessageType} failed on attempt {Attempts}.",
                    message.Id,
                    message.Type,
                    message.Attempts);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task CleanupIfDueAsync(OutboxOptions options, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        if (now - _lastCleanup < TimeSpan.FromMinutes(options.CleanupIntervalMinutes))
        {
            return;
        }

        using var scope = scopeFactory.CreateScope();

        foreach (var registration in registrations)
        {
            if (scope.ServiceProvider.GetService(registration.ContextType) is not ModuleDbContext dbContext)
            {
                continue;
            }

            var cutoff = now.AddDays(-options.RetentionDays);

            var deleted = await dbContext.OutboxMessages
                .Where(message => message.Status != OutboxMessageStatus.Pending && message.OccurredAt < cutoff)
                .ExecuteDeleteAsync(cancellationToken);

            if (deleted > 0)
            {
                logger.LogInformation(
                    "Deleted {Count} processed outbox messages from schema {Schema}.",
                    deleted,
                    dbContext.Schema);
            }
        }

        _lastCleanup = now;
    }

    private static async Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(delay, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Normal shutdown.
        }
    }
}
