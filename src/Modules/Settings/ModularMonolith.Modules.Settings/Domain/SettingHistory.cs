namespace ModularMonolith.Modules.Settings.Domain;

public sealed class SettingHistory
{
    private SettingHistory()
    {
    }

    private SettingHistory(
        Guid id,
        Guid settingId,
        string key,
        SettingDataType dataType,
        SettingScope scope,
        string module,
        Guid tenantId,
        Guid userId,
        string? oldValue,
        string? newValue,
        SettingChangeType changeType,
        int version,
        Guid? changedBy,
        DateTimeOffset changedAt)
    {
        Id = id;
        SettingId = settingId;
        Key = key;
        DataType = dataType;
        Scope = scope;
        Module = module;
        TenantId = tenantId;
        UserId = userId;
        OldValue = oldValue;
        NewValue = newValue;
        ChangeType = changeType;
        Version = version;
        ChangedBy = changedBy;
        ChangedAt = changedAt;
    }

    public Guid Id { get; private set; }

    public Guid SettingId { get; private set; }

    public string Key { get; private set; } = string.Empty;

    public SettingDataType DataType { get; private set; }

    public SettingScope Scope { get; private set; }

    public string Module { get; private set; } = string.Empty;

    public Guid TenantId { get; private set; }

    public Guid UserId { get; private set; }

    public string? OldValue { get; private set; }

    public string? NewValue { get; private set; }

    public SettingChangeType ChangeType { get; private set; }

    public int Version { get; private set; }

    public Guid? ChangedBy { get; private set; }

    public DateTimeOffset ChangedAt { get; private set; }

    public static SettingHistory Record(
        Setting setting,
        string? oldValue,
        string? newValue,
        SettingChangeType changeType,
        Guid? changedBy,
        DateTimeOffset changedAt)
    {
        ArgumentNullException.ThrowIfNull(setting);

        return new SettingHistory(
            Guid.CreateVersion7(),
            setting.Id,
            setting.Key,
            setting.DataType,
            setting.Scope,
            setting.Module,
            setting.TenantId,
            setting.UserId,
            oldValue,
            newValue,
            changeType,
            setting.Version,
            changedBy,
            changedAt);
    }
}
