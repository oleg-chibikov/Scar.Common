using System;
using System.Threading.Tasks;

namespace Scar.Common.Caching;

public interface IApplicationCache
{
    Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory);

    Task<T> GetOrAddWithSlidingExpirationAsync<T>(string key, Func<Task<T>> addItemFactory, TimeSpan slidingExpiration);

    Task<T> GetOrAddWithAbsoluteExpirationAsync<T>(string key, Func<Task<T>> addItemFactory, DateTimeOffset absoluteExpiration);

    Task<T> GetOrAddWithAbsoluteExpirationAsync<T>(string key, Func<Task<T>> addItemFactory, TimeSpan absoluteExpiration);

    T GetOrAdd<T>(string key, Func<T> addItemFactory);

    T GetOrAddWithSlidingExpiration<T>(string key, Func<T> addItemFactory, TimeSpan slidingExpiration);

    T GetOrAddWithAbsoluteExpiration<T>(string key, Func<T> addItemFactory, DateTimeOffset absoluteExpiration);

    T GetOrAddWithAbsoluteExpiration<T>(string key, Func<T> addItemFactory, TimeSpan absoluteExpiration);
}