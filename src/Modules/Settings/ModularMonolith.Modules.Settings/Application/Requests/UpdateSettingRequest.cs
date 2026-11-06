namespace ModularMonolith.Modules.Settings.Application.Requests;

public sealed record UpdateSettingRequest(
    string? Value,
    string? Description = null,
    string? Category = null,
    bool IsRequired = false,
    bool IsReadOnly = false,
    IReadOnlyList<string>? AllowedValues = null,
    string? ValidationPattern = null);
