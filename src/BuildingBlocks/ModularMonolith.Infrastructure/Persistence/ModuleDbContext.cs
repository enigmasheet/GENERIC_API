using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Infrastructure.Persistence.Outbox;
using ModularMonolith.SharedKernel.Abstractions;
using ModularMonolith.SharedKernel.Domain;

namespace ModularMonolith.Infrastructure.Persistence;

public abstract class ModuleDbContext : DbContext
{
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;
    private readonly TimeProvider _timeProvider;

    protected ModuleDbContext(
        DbContextOptions options,
        ICurrentUser currentUser,
        ITenantContext tenantContext,
        TimeProvider timeProvider)
        : base(options)
    {
        _currentUser = currentUser;
        _tenantContext = tenantContext;
        _timeProvider = timeProvider;
    }

    public const string OutboxTableName = "outbox_messages";

    public abstract string Schema { get; }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        ConfigureOutbox(modelBuilder);
        ConfigureSoftDeleteFilters(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyAuditInformation();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditInformation();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private static void ConfigureOutbox(ModelBuilder modelBuilder)
    {
        var outbox = modelBuilder.Entity<OutboxMessage>();

        outbox.ToTable(OutboxTableName);
        outbox.HasKey(message => message.Id);
        outbox.Property(message => message.Type)
            .HasMaxLength(500)
            .IsRequired();
        outbox.Property(message => message.Payload)
            .HasColumnType("jsonb")
            .IsRequired();
        outbox.Property(message => message.Status)
            .HasConversion<int>()
            .IsRequired();
        outbox.Property(message => message.Error)
            .HasMaxLength(4000);
        outbox.Property(message => message.CorrelationId)
            .HasMaxLength(128);
        outbox.HasIndex(message => new { message.Status, message.NextAttemptAt });
        outbox.HasIndex(message => message.OccurredAt);
    }

    private static void ConfigureSoftDeleteFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var softDeleteProperty = Expression.Property(
                Expression.Convert(parameter, typeof(ISoftDeletable)),
                nameof(ISoftDeletable.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(softDeleteProperty), parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }

    private void ApplyAuditInformation()
    {
        var now = _timeProvider.GetUtcNow();
        var userId = _currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = now;
            entry.Entity.DeletedBy = userId;
        }
    }

    protected Guid CurrentTenantId => _tenantContext.TenantId;

    protected Guid? CurrentUserId => _currentUser.UserId;

    protected bool IsMultiTenant => _tenantContext.IsMultiTenant;
}
