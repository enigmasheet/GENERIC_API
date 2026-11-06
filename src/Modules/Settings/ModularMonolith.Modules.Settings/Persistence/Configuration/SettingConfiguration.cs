using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularMonolith.Modules.Settings.Domain;

namespace ModularMonolith.Modules.Settings.Persistence.Configuration;

public sealed class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("Settings");

        builder.HasKey(setting => setting.Id);

        builder.Property(setting => setting.Key)
            .HasMaxLength(SettingRules.MaxKeyLength)
            .IsRequired();

        builder.Property(setting => setting.NormalizedKey)
            .HasMaxLength(SettingRules.MaxKeyLength)
            .IsRequired();

        builder.Property(setting => setting.Value)
            .HasMaxLength(SettingRules.MaxValueLength);

        builder.Property(setting => setting.DataType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(setting => setting.Scope)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(setting => setting.Module)
            .HasMaxLength(SettingRules.MaxModuleLength)
            .IsRequired();

        builder.Property(setting => setting.Description)
            .HasMaxLength(SettingRules.MaxDescriptionLength);

        builder.Property(setting => setting.Category)
            .HasMaxLength(SettingRules.MaxCategoryLength);

        builder.Property(setting => setting.AllowedValuesJson)
            .HasColumnName("AllowedValues")
            .HasMaxLength(SettingRules.MaxValueLength);

        builder.Property(setting => setting.ValidationPattern)
            .HasMaxLength(SettingRules.MaxValidationPatternLength);

        builder.Property(setting => setting.Version)
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(setting => setting.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Ignore(setting => setting.DomainEvents);

        builder.HasIndex(setting => new
            {
                setting.NormalizedKey,
                setting.Scope,
                setting.Module,
                setting.TenantId,
                setting.UserId
            })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IX_Settings_UniqueKey");

        builder.HasIndex(setting => new { setting.Scope, setting.Module });
        builder.HasIndex(setting => new { setting.TenantId, setting.UserId });
        builder.HasIndex(setting => setting.Category);
    }
}
