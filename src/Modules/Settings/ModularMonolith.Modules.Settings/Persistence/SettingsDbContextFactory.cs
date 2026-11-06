using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ModularMonolith.Infrastructure.Persistence.DesignTime;

namespace ModularMonolith.Modules.Settings.Persistence;

public sealed class SettingsDbContextFactory : IDesignTimeDbContextFactory<SettingsDbContext>
{
    private const string LocalDevelopmentConnectionString =
        "Host=localhost;Port=5432;Database=modular_monolith;Username=postgres;Password=postgres";

    public SettingsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
            ?? LocalDevelopmentConnectionString;

        var options = new DbContextOptionsBuilder<SettingsDbContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable(
                    "__ef_migrations_history",
                    SettingsDbContext.SchemaName);
            })
            .Options;

        return new SettingsDbContext(
            options,
            new DesignTimeCurrentUser(),
            new DesignTimeTenantContext(),
            TimeProvider.System);
    }
}
