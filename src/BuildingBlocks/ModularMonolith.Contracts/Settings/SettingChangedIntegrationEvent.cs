using ModularMonolith.SharedKernel.Abstractions;

namespace ModularMonolith.Contracts.Settings;

public sealed record SettingChangedIntegrationEvent : IntegrationEvent
{
    public required string Key { get; init; }

    public string? OldValue { get; init; }

    public string? NewValue { get; init; }

    public required string DataType { get; init; }

    public Guid? ChangedBy { get; init; }
}
