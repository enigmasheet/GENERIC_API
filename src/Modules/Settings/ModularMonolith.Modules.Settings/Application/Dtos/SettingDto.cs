using ModularMonolith.Modules.Settings.Domain;

namespace ModularMonolith.Modules.Settings.Application.Dtos;

public sealed record SettingDto(
    Guid Id,
    string Key,
    string? Value,
    SettingDataType DataType,
    SettingScope Scope,
    string? Module,
    Guid? TenantId,
    Guid? UserId,
    string? Description,
    string? Category,
    bool IsRequired,
    bool IsReadOnly,
    IReadOnlyList<string> AllowedValues,
    string? ValidationPattern,
    int Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    public static SettingDto From(Setting setting)
    {
        return new SettingDto(
            setting.Id,
            setting.Key,
            setting.Value,
            setting.DataType,
            setting.Scope,
            string.IsNullOrEmpty(setting.Module) ? null : setting.Module,
            setting.TenantId == Guid.Empty ? null : setting.TenantId,
            setting.UserId == Guid.Empty ? null : setting.UserId,
            setting.Description,
            setting.Category,
            setting.IsRequired,
            setting.IsReadOnly,
            SettingValueValidator.ParseAllowedValues(setting.AllowedValuesJson),
            setting.ValidationPattern,
            setting.Version,
            setting.CreatedAt,
            setting.UpdatedAt);
    }
}
