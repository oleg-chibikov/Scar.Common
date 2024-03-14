using System;
using System.Threading.Tasks;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;

namespace Scar.Common.Caching;

public sealed class ApplicationCache : IApplicationCache, IDisposable
{
    readonly CachingService _internalCache;
    readonly MemoryCacheProvider _cacheProvider;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "MemoryCache is disposed by cache provider")]
    public ApplicationCache()
    {
        _cacheProvider = new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions()));
        _internalCache = new CachingService(_cacheProvider);
    }

    public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory)
    {
        return await _internalCache.GetOrAddAsync(key, addItemFactory).ConfigureAwait(false);
    }

    public async Task<T> GetOrAddWithSlidingExpirationAsync<T>(string key, Func<Task<T>> addItemFactory, TimeSpan slidingExpiration)
    {
        return await _internalCache.GetOrAddAsync(key, addItemFactory, slidingExpiration).ConfigureAwait(false);
    }

    public async Task<T> GetOrAddWithAbsoluteExpirationAsync<T>(string key, Func<Task<T>> addItemFactory, DateTimeOffset absoluteExpiration)
    {
        return await _internalCache.GetOrAddAsync(key, addItemFactory, absoluteExpiration).ConfigureAwait(false);
    }

    public async Task<T> GetOrAddWithAbsoluteExpirationAsync<T>(string key, Func<Task<T>> addItemFactory, TimeSpan absoluteExpiration)
    {
        return await _internalCache.GetOrAddAsync(key, addItemFactory, DateTime.Now.Add(absoluteExpiration)).ConfigureAwait(false);
    }

    public T GetOrAdd<T>(string key, Func<T> addItemFactory)
    {
        return _internalCache.GetOrAdd(key, addItemFactory);
    }

    public T GetOrAddWithSlidingExpiration<T>(string key, Func<T> addItemFactory, TimeSpan slidingExpiration)
    {
        return _internalCache.GetOrAdd(key, addItemFactory, slidingExpiration);
    }

    public T GetOrAddWithAbsoluteExpiration<T>(string key, Func<T> addItemFactory, DateTimeOffset absoluteExpiration)
    {
        return _internalCache.GetOrAdd(key, addItemFactory, absoluteExpiration);
    }

    public T GetOrAddWithAbsoluteExpiration<T>(string key, Func<T> addItemFactory, TimeSpan absoluteExpiration)
    {
        return _internalCache.GetOrAdd(key, addItemFactory, absoluteExpiration);
    }

    public void Dispose()
    {
        _cacheProvider.Dispose();
    }
}
