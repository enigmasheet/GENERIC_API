namespace ModularMonolith.Contracts.Settings;

public interface ISettingsReader
{
    Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default);

    Task<T?> GetValueAsync<T>(string key, CancellationToken cancellationToken = default);
}
