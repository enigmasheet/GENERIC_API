using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Modules.Settings.Domain;

namespace ModularMonolith.Modules.Settings.Persistence.Configuration;

public sealed class SettingHistoryConfiguration : IEntityTypeConfiguration<SettingHistory>
{
    public void Configure(EntityTypeBuilder<SettingHistory> builder)
    {
        builder.ToTable("SettingHistory");

        builder.HasKey(history => history.Id);

        builder.Property(history => history.Key)
            .HasMaxLength(SettingRules.MaxKeyLength)
            .IsRequired();

        builder.Property(history => history.Module)
            .HasMaxLength(SettingRules.MaxModuleLength)
            .IsRequired();

        builder.Property(history => history.DataType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(history => history.Scope)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(history => history.ChangeType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(history => history.OldValue)
            .HasMaxLength(SettingRules.MaxValueLength);

        builder.Property(history => history.NewValue)
            .HasMaxLength(SettingRules.MaxValueLength);

        builder.HasOne<Setting>()
            .WithMany()
            .HasForeignKey(history => history.SettingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(history => new { history.SettingId, history.ChangedAt })
            .HasDatabaseName("IX_SettingHistory_SettingId_ChangedAt");

        builder.HasIndex(history => new { history.TenantId, history.UserId });
    }
}
