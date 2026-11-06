using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Contracts.Settings;
using ModularMonolith.Infrastructure.Caching;
using ModularMonolith.Infrastructure.Persistence.Outbox;
using ModularMonolith.Modules.Settings.Application.Dtos;
using ModularMonolith.Modules.Settings.Application.Requests;
using ModularMonolith.Modules.Settings.Domain;
using ModularMonolith.Modules.Settings.Persistence;
using ModularMonolith.SharedKernel.Abstractions;
using ModularMonolith.SharedKernel.Exceptions;
using ModularMonolith.SharedKernel.Pagination;

namespace ModularMonolith.Modules.Settings.Application;

public sealed class SettingsService(
    SettingsDbContext dbContext,
    ICacheService cacheService,
    ICurrentUser currentUser,
    ITenantContext tenantContext,
    ICorrelationContext correlationContext,
    TimeProvider timeProvider) : ISettingsService
{
    private bool CanManageOtherUsers =>
        currentUser.Permissions.Contains(SettingsPermissions.Manage, StringComparer.Ordinal);

    public async Task<PagedResult<SettingDto>> ListAsync(
        SettingsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var pagedRequest = query.ToPagedRequest();
        var settings = dbContext.Settings.AsNoTracking();

        if (!CanManageOtherUsers)
        {
            var currentUserId = currentUser.UserId ?? Guid.Empty;
            settings = settings.Where(setting =>
                setting.UserId == Guid.Empty || setting.UserId == currentUserId);
        }

        if (query.Scope is not null)
        {
            settings = settings.Where(setting => setting.Scope == query.Scope.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Module))
        {
            var module = query.Module.Trim();
            settings = settings.Where(setting => setting.Module == module);
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var category = query.Category.Trim();
            settings = settings.Where(setting => setting.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(pagedRequest.Search))
        {
            var pattern = $"%{EscapeLikePattern(pagedRequest.Search.Trim())}%";
            settings = settings.Where(setting =>
                EF.Functions.ILike(setting.Key, pattern)
                || (setting.Description != null && EF.Functions.ILike(setting.Description, pattern))
                || (setting.Category != null && EF.Functions.ILike(setting.Category, pattern)));
        }

        var totalCount = await settings.CountAsync(cancellationToken);

        settings = ApplySort(settings, pagedRequest.SortBy, pagedRequest.SortDirection);

        var items = await settings
            .Skip(pagedRequest.Skip)
            .Take(pagedRequest.Take)
            .ToListAsync(cancellationToken);

        return new PagedResult<SettingDto>(
            items.Select(SettingDto.From).ToArray(),
            Math.Max(pagedRequest.Page, 1),
            pagedRequest.Take,
            totalCount);
    }

    public async Task<SettingDto> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var setting = await dbContext.Settings
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken)
            ?? throw NotFoundException.For("Setting", id);

        EnsureCanAccess(setting);

        return SettingDto.From(setting);
    }

    public async Task<SettingDto> CreateAsync(
        CreateSettingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tenantId = ResolveTenantId(request.Scope, request.TenantId);
        var userId = ResolveUserId(request.Scope, request.UserId);
        var module = request.Module?.Trim() ?? string.Empty;
        var allowedValuesJson = SettingValueValidator.SerializeAllowedValues(request.AllowedValues);
        var validationPattern = NormalizePattern(request.ValidationPattern);

        EnsureValueValid(
            request.DataType,
            request.Value,
            request.IsRequired,
            allowedValuesJson,
            validationPattern);

        var duplicateExists = await dbContext.Settings.AnyAsync(setting =>
            setting.NormalizedKey == SettingRules.NormalizeKey(request.Key)
            && setting.Scope == request.Scope
            && setting.Module == module
            && setting.TenantId == tenantId
            && setting.UserId == userId,
            cancellationToken);

        if (duplicateExists)
        {
            throw new ConflictException(
                $"A setting with key '{request.Key.Trim()}' already exists for the selected scope.",
                "settings.duplicate_key");
        }

        SettingDto result;

        try
        {
            result = await ExecuteInTransactionAsync(async () =>
            {
                var now = timeProvider.GetUtcNow();
                var setting = Setting.Create(
                    request.Key,
                    request.Value,
                    request.DataType,
                    request.Scope,
                    module,
                    tenantId,
                    userId,
                    request.Description,
                    request.Category,
                    request.IsRequired,
                    request.IsReadOnly,
                    allowedValuesJson,
                    validationPattern,
                    now);

                var history = SettingHistory.Record(
                    setting,
                    null,
                    setting.Value,
                    SettingChangeType.Created,
                    currentUser.UserId,
                    now);

                dbContext.Settings.Add(setting);
                dbContext.SettingHistories.Add(history);
                dbContext.OutboxMessages.Add(CreateOutboxMessage(setting, null));

                await dbContext.SaveChangesAsync(cancellationToken);

                return SettingDto.From(setting);
            }, cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw new ConflictException(
                $"A setting with key '{request.Key.Trim()}' already exists for the selected scope.",
                "settings.duplicate_key");
        }

        await InvalidateCacheAsync(cancellationToken);

        return result;
    }

    public async Task<SettingDto> UpdateAsync(
        Guid id,
        UpdateSettingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var allowedValuesJson = SettingValueValidator.SerializeAllowedValues(request.AllowedValues);
        var validationPattern = NormalizePattern(request.ValidationPattern);

        SettingDto result;

        try
        {
            result = await ExecuteInTransactionAsync(async () =>
            {
                var setting = await dbContext.Settings
                    .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken)
                    ?? throw NotFoundException.For("Setting", id);

                EnsureCanAccess(setting);

                EnsureValueValid(
                    setting.DataType,
                    request.Value,
                    request.IsRequired,
                    allowedValuesJson,
                    validationPattern);

                if (setting.HasSameState(
                        request.Value,
                        request.Description,
                        request.Category,
                        request.IsRequired,
                        request.IsReadOnly,
                        allowedValuesJson,
                        validationPattern))
                {
                    return SettingDto.From(setting);
                }

                var oldValue = setting.Value;
                var now = timeProvider.GetUtcNow();

                setting.Update(
                    request.Value,
                    request.Description,
                    request.Category,
                    request.IsRequired,
                    request.IsReadOnly,
                    allowedValuesJson,
                    validationPattern);

                var history = SettingHistory.Record(
                    setting,
                    oldValue,
                    setting.Value,
                    SettingChangeType.Updated,
                    currentUser.UserId,
                    now);

                dbContext.SettingHistories.Add(history);
                dbContext.OutboxMessages.Add(CreateOutboxMessage(setting, oldValue));

                await dbContext.SaveChangesAsync(cancellationToken);

                return SettingDto.From(setting);
            }, cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The setting was modified by another request. Reload it and retry.",
                "settings.concurrency_conflict");
        }

        await InvalidateCacheAsync(cancellationToken);

        return result;
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await ExecuteInTransactionAsync<object?>(async () =>
        {
            var setting = await dbContext.Settings
                .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken)
                ?? throw NotFoundException.For("Setting", id);

            EnsureCanAccess(setting);

            if (setting.IsReadOnly)
            {
                throw new DomainException(
                    $"Setting '{setting.Key}' is read-only and cannot be deleted.",
                    "settings.read_only");
            }

            var now = timeProvider.GetUtcNow();
            var oldValue = setting.Value;

            dbContext.Settings.Remove(setting);
            dbContext.SettingHistories.Add(SettingHistory.Record(
                setting,
                oldValue,
                null,
                SettingChangeType.Deleted,
                currentUser.UserId,
                now));
            dbContext.OutboxMessages.Add(CreateOutboxMessage(setting, oldValue));

            await dbContext.SaveChangesAsync(cancellationToken);

            return null;
        }, cancellationToken);

        await InvalidateCacheAsync(cancellationToken);
    }

    public async Task<PagedResult<SettingHistoryDto>> GetHistoryAsync(
        Guid id,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var setting = await dbContext.Settings
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken)
            ?? throw NotFoundException.For("Setting", id);

        EnsureCanAccess(setting);

        var history = dbContext.SettingHistories
            .AsNoTracking()
            .Where(entry => entry.SettingId == id);

        var totalCount = await history.CountAsync(cancellationToken);

        var items = await history
            .OrderByDescending(entry => entry.ChangedAt)
            .ThenByDescending(entry => entry.Version)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        return new PagedResult<SettingHistoryDto>(
            items.Select(SettingHistoryDto.From).ToArray(),
            Math.Max(request.Page, 1),
            request.Take,
            totalCount);
    }

    private Guid ResolveTenantId(SettingScope scope, Guid? requestedTenantId)
    {
        if (scope is SettingScope.Global or SettingScope.Module)
        {
            if (requestedTenantId is not null)
            {
                throw new DomainException(
                    "Global and module settings cannot specify a tenant owner.",
                    "settings.scope_owner_invalid");
            }

            return Guid.Empty;
        }

        if (scope == SettingScope.User)
        {
            if (requestedTenantId is not null)
            {
                throw new DomainException(
                    "User settings derive their tenant from the current context.",
                    "settings.scope_owner_invalid");
            }

            return tenantContext.IsMultiTenant ? tenantContext.TenantId : Guid.Empty;
        }

        var tenantId = requestedTenantId ?? tenantContext.TenantId;

        if (tenantId == Guid.Empty)
        {
            throw new DomainException(
                "A tenant identifier is required for tenant-scoped settings.",
                "settings.tenant_required");
        }

        if (requestedTenantId is not null
            && requestedTenantId.Value != tenantContext.TenantId
            && !CanManageOtherUsers)
        {
            throw new ForbiddenException(
                "You cannot manage settings for another tenant.");
        }

        return tenantId;
    }

    private Guid ResolveUserId(SettingScope scope, Guid? requestedUserId)
    {
        if (scope != SettingScope.User)
        {
            if (requestedUserId is not null)
            {
                throw new DomainException(
                    "Only user-scoped settings can specify a user owner.",
                    "settings.scope_owner_invalid");
            }

            return Guid.Empty;
        }

        var userId = requestedUserId ?? currentUser.UserId;

        if (userId is null || userId == Guid.Empty)
        {
            throw new DomainException(
                "User-scoped settings require an authenticated user or an explicit user identifier.",
                "settings.user_required");
        }

        if (requestedUserId is not null
            && requestedUserId != currentUser.UserId
            && !CanManageOtherUsers)
        {
            throw new ForbiddenException(
                "You cannot manage settings for another user.");
        }

        return userId.Value;
    }

    private void EnsureCanAccess(Setting setting)
    {
        if (setting.TenantId != Guid.Empty
            && setting.TenantId != tenantContext.TenantId
            && !CanManageOtherUsers)
        {
            throw NotFoundException.For("Setting", setting.Id);
        }

        if (setting.Scope == SettingScope.User
            && setting.UserId != Guid.Empty
            && setting.UserId != (currentUser.UserId ?? Guid.Empty)
            && !CanManageOtherUsers)
        {
            throw NotFoundException.For("Setting", setting.Id);
        }
    }

    private static void EnsureValueValid(
        SettingDataType dataType,
        string? value,
        bool isRequired,
        string? allowedValuesJson,
        string? validationPattern)
    {
        var errors = SettingValueValidator.Validate(
            dataType,
            value,
            isRequired,
            allowedValuesJson,
            validationPattern);

        if (errors.Count == 0)
        {
            return;
        }

        var failures = errors
            .SelectMany(entry => entry.Value.Select(message =>
                new ValidationFailure(entry.Key, message)))
            .ToArray();

        throw new ValidationException(failures);
    }

    private OutboxMessage CreateOutboxMessage(
        Setting setting,
        string? oldValue)
    {
        var integrationEvent = new SettingChangedIntegrationEvent
        {
            Key = setting.Key,
            OldValue = oldValue,
            NewValue = setting.Value,
            DataType = setting.DataType.ToString(),
            ChangedBy = currentUser.UserId,
            CorrelationId = correlationContext.CorrelationId,
            TenantId = setting.TenantId == Guid.Empty ? null : setting.TenantId
        };

        return OutboxMessage.Create(integrationEvent);
    }

    private async Task<T> ExecuteInTransactionAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            dbContext.ChangeTracker.Clear();

            await using var transaction = await dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            var result = await operation();

            await transaction.CommitAsync(cancellationToken);

            return result;
        });
    }

    private Task InvalidateCacheAsync(CancellationToken cancellationToken)
    {
        return cacheService.InvalidateGroupAsync(SettingsCache.Group, cancellationToken);
    }

    private static string? NormalizePattern(string? pattern)
    {
        return string.IsNullOrWhiteSpace(pattern) ? null : pattern;
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }

    private static IQueryable<Setting> ApplySort(
        IQueryable<Setting> query,
        string? sortBy,
        SortDirection direction)
    {
        var descending = direction == SortDirection.Descending;

        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "scope" => descending
                ? query.OrderByDescending(setting => setting.Scope).ThenByDescending(setting => setting.Key)
                : query.OrderBy(setting => setting.Scope).ThenBy(setting => setting.Key),
            "category" => descending
                ? query.OrderByDescending(setting => setting.Category).ThenByDescending(setting => setting.Key)
                : query.OrderBy(setting => setting.Category).ThenBy(setting => setting.Key),
            "createdat" => descending
                ? query.OrderByDescending(setting => setting.CreatedAt).ThenByDescending(setting => setting.Key)
                : query.OrderBy(setting => setting.CreatedAt).ThenBy(setting => setting.Key),
            "updatedat" => descending
                ? query.OrderByDescending(setting => setting.UpdatedAt).ThenByDescending(setting => setting.Key)
                : query.OrderBy(setting => setting.UpdatedAt).ThenBy(setting => setting.Key),
            "version" => descending
                ? query.OrderByDescending(setting => setting.Version).ThenByDescending(setting => setting.Key)
                : query.OrderBy(setting => setting.Version).ThenBy(setting => setting.Key),
            _ => descending
                ? query.OrderByDescending(setting => setting.Key)
                : query.OrderBy(setting => setting.Key)
        };
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is Npgsql.PostgresException
        {
            SqlState: Npgsql.PostgresErrorCodes.UniqueViolation
        };
    }
}
