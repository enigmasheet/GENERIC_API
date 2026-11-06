using ModularMonolith.Modules.Settings.Domain;

namespace ModularMonolith.Modules.Settings.Application.Dtos;

public sealed record SettingHistoryDto(
    Guid Id,
    Guid SettingId,
    string Key,
    SettingDataType DataType,
    SettingScope Scope,
    string? Module,
    Guid? TenantId,
    Guid? UserId,
    string? OldValue,
    string? NewValue,
    SettingChangeType ChangeType,
    int Version,
    Guid? ChangedBy,
    DateTimeOffset ChangedAt)
{
    public static SettingHistoryDto From(SettingHistory history)
    {
        return new SettingHistoryDto(
            history.Id,
            history.SettingId,
            history.Key,
            history.DataType,
            history.Scope,
            string.IsNullOrEmpty(history.Module) ? null : history.Module,
            history.TenantId == Guid.Empty ? null : history.TenantId,
            history.UserId == Guid.Empty ? null : history.UserId,
            history.OldValue,
            history.NewValue,
            history.ChangeType,
            history.Version,
            history.ChangedBy,
            history.ChangedAt);
    }
}
