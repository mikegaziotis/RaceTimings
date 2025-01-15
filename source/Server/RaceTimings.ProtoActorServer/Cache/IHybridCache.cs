using System.Collections.Concurrent;
using CSharpFunctionalExtensions;
using ProtoBuf;
using StackExchange.Redis;

namespace RaceTimings.ProtoActorServer.Cache;

public interface IHybridCache
{
    ValueTask<bool> KeyExistsAsync(string key);
    ValueTask<string[]> GetAllKeysAsync(string keyCollection);
    
    ValueTask<Maybe<TEntity>> GetOrCreateAsync<TEntity>(string key, Func<Task<Maybe<TEntity>>> factory, string? keyCollection = null) where TEntity : class;

    ValueTask<Maybe<TEntity>> GetAsync<TEntity>(string key) where TEntity : class;
    
    ValueTask SetAsync<TEntity>(string key, TEntity value, string? keyCollection = null) where TEntity : class;
    
    ValueTask RemoveAsync(string key, string? keyCollection = null);
}

public class HybridCache(IConnectionMultiplexer redisConnection): IHybridCache
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();
    private readonly ConcurrentDictionary<string, object> _localCache = new();
    
    public async ValueTask<bool> KeyExistsAsync(string key)
    {
        return _localCache.ContainsKey(key) || await CheckKeyInDistributedCache(key);

        async Task<bool> CheckKeyInDistributedCache(string s)
        {
            var database = redisConnection.GetDatabase();
            return await database.KeyExistsAsync(s);
        }
    }

    public async ValueTask<string[]> GetAllKeysAsync(string keyCollection)
    {
        var database = redisConnection.GetDatabase();
        var redisValues = await database.SetMembersAsync(keyCollection);
        return redisValues.Select(x => x.ToString()).ToArray();
    }

    public async ValueTask<Maybe<TEntity>> GetOrCreateAsync<TEntity>(string key, Func<Task<Maybe<TEntity>>> factory, string? collectionKey) where TEntity : class
    {
        // Get or create the SemaphoreSlim lock for this key
        var semaphore = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        try
        {
            // Wait asynchronously to acquire the lock
            await semaphore.WaitAsync(new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token);

            // Check the local cache first
            if (_localCache.TryGetValue(key, out var cachedValue))
            {
                return cachedValue switch
                {
                    TEntity entity => entity, 
                    _ => Maybe<TEntity>.None 
                };
            }

            // Check the distributed cache
            var maybeValue = await GetItemFromRedis<TEntity>(key);
            if (maybeValue.HasValue)
            {
                // Add to local cache and return the value
                _localCache[key] = maybeValue.Value;
                return maybeValue.Value;
            }

            // If not in cache, run the factory to create the value
            var result = await factory();
            if (result.HasValue)
            {
                // If factory returned value, store in both distributed and local caches
                await SetAsync(key, result.Value, collectionKey);
                _localCache[key] = result.Value;
            }
            return result;
        }
        finally
        {
            // Release the lock and clean up the semaphore from the dictionary
            semaphore.Release();
            Locks.TryRemove(key, out _);
        }
    }

    public async ValueTask<Maybe<TEntity>> GetAsync<TEntity>(string key) where TEntity : class
    {
        if (_localCache.TryGetValue(key, out var value))
        {
            return value switch
            {
                TEntity entity => Maybe<TEntity>.From(entity),
                _ => Maybe<TEntity>.None
            };
        }
        return await GetItemFromRedis<TEntity>(key);
    }
    
    public async ValueTask SetAsync<TEntity>(string key, TEntity value, string? keyCollection) where TEntity : class
    {
        var database = redisConnection.GetDatabase();
        using var memoryStream = new MemoryStream();
        Serializer.Serialize(memoryStream, value);
        await database.StringSetAsync(key, memoryStream.ToArray());
        if(keyCollection is not null)
            await database.SetAddAsync(keyCollection, key);
        _localCache.AddOrUpdate(key, value, (_, _) => value);    
    }

    public async ValueTask RemoveAsync(string key, string? keyCollection)
    {
        var database = redisConnection.GetDatabase();
        await database.KeyDeleteAsync(key);
        if(keyCollection is not null)
            await database.SetRemoveAsync(keyCollection, key);
        _localCache.TryRemove(key, out _);
    }

    private async Task<Maybe<TEntity>> GetItemFromRedis<TEntity>(string key) where TEntity : class
    {
        var database = redisConnection.GetDatabase();
        var redisValue = await database.StringGetAsync(key);
        if(!redisValue.HasValue || redisValue.IsNull)
            return Maybe<TEntity>.None;
        using var memoryStream = new MemoryStream(redisValue!);
        var result = Serializer.Deserialize<TEntity>(memoryStream);
        _localCache.AddOrUpdate(key, result, (_, _) => result);
        return result;
    }
}