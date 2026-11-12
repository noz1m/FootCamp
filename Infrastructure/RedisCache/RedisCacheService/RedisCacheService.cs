using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure.RedisCache.RedisCacheService;

public class RedisCacheService(
    IConnectionMultiplexer redis,
    ILogger<RedisCacheService> logger) : IRedisCacheService
{
    private readonly IDatabase _db = redis.GetDatabase();

    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(30);

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(value.ToString()!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Redis GET error for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, expiry ?? DefaultExpiry);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Redis SET error for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Redis REMOVE error for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var server = redis.GetServer(redis.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern).ToArray();
            if (keys.Length > 0)
                await _db.KeyDeleteAsync(keys);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Redis REMOVE BY PATTERN error for pattern: {Pattern}", pattern);
        }
    }
}