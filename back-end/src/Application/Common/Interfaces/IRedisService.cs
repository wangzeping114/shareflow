namespace ShareFlow.Application.Common.Interfaces;

public interface IRedisService
{
    Task SetStringAsync(string key, string value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

    Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> KeyExistsAsync(string key, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}