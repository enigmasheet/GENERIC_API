using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularMonolith.Infrastructure.Options;

namespace ModularMonolith.Infrastructure.Persistence;

public static class DatabaseMigrationExtensions
{
    public static async Task MigrateModuleDatabasesAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        var options = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        if (!options.AutoMigrate)
        {
            return;
        }

        var logger = serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseMigration");

        var registrations = serviceProvider.GetServices<ModuleDbContextRegistration>();

        foreach (var registration in registrations)
        {
            using var scope = serviceProvider.CreateScope();

            if (scope.ServiceProvider.GetService(registration.ContextType) is not ModuleDbContext dbContext)
            {
                throw new InvalidOperationException(
                    $"Module DbContext '{registration.ContextType.FullName}' is not registered in the service container.");
            }

            logger.LogInformation(
                "Applying migrations for {ContextType} on schema {Schema}.",
                registration.ContextType.Name,
                dbContext.Schema);

            await dbContext.Database.MigrateAsync(cancellationToken);
        }
    }
}
