using Microsoft.EntityFrameworkCore;
using ModularMonolith.Infrastructure.Persistence;
using ModularMonolith.Modules.Settings.Domain;
using ModularMonolith.SharedKernel.Abstractions;

namespace ModularMonolith.Modules.Settings.Persistence;

public sealed class SettingsDbContext : ModuleDbContext
{
    public const string SchemaName = "settings";

    public SettingsDbContext(
        DbContextOptions<SettingsDbContext> options,
        ICurrentUser currentUser,
        ITenantContext tenantContext,
        TimeProvider timeProvider)
        : base(options, currentUser, tenantContext, timeProvider)
    {
    }

    public override string Schema => SchemaName;

    public DbSet<Setting> Settings => Set<Setting>();

    public DbSet<SettingHistory> SettingHistories => Set<SettingHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SettingsDbContext).Assembly);

        modelBuilder.Entity<Setting>()
            .HasQueryFilter(setting =>
                !setting.IsDeleted
                && (setting.TenantId == Guid.Empty
                    || setting.TenantId == CurrentTenantId));
    }
}
