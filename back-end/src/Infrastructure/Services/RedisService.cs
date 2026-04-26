using ShareFlow.Application.Common.Interfaces;
using StackExchange.Redis;

namespace ShareFlow.Infrastructure.Services;

public class RedisService(IConnectionMultiplexer connectionMultiplexer) : IRedisService
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

    public async Task SetStringAsync(string key, string value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        await _database.StringSetAsync(key, value, expiry);
    }

    public async Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task<bool> KeyExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _database.KeyExistsAsync(key);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _database.KeyDeleteAsync(key);
    }
}