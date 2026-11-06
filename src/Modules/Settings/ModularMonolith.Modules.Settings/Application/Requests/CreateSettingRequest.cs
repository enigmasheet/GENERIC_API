using ModularMonolith.Modules.Settings.Domain;

namespace ModularMonolith.Modules.Settings.Application.Requests;

public sealed record CreateSettingRequest(
    string Key,
    string? Value,
    SettingDataType DataType,
    SettingScope Scope = SettingScope.Global,
    string? Module = null,
    Guid? TenantId = null,
    Guid? UserId = null,
    string? Description = null,
    string? Category = null,
    bool IsRequired = false,
    bool IsReadOnly = false,
    IReadOnlyList<string>? AllowedValues = null,
    string? ValidationPattern = null);
