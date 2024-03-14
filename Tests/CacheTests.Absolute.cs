using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Scar.Common.Caching;

namespace Scar.Common.Tests.Tests;

public partial class CacheTests
{
    [Test]
    public async Task UsesCachedValueUntilTimeHasPassed_WhenAbsoluteExpirationIsSpecifiedAsync()
    {
        // Arrange - Setup ApplicationCache, Mock & put initial cache value that will expire in 100 milliseconds
        using var cache = new ApplicationCache();
        const string cacheName = "CacheTests-AbsoluteExp1";

        const string initialCacheValue = "Message-0";
        var functionMock = new Mock<Func<Task<string>>>();
        functionMock.Setup(x => x()).Returns(Task.FromResult(initialCacheValue));
        await cache.GetOrAddWithAbsoluteExpirationAsync(cacheName, functionMock.Object, CacheDuration).ConfigureAwait(false);

        functionMock.Invocations.Clear();
        await Task.Delay(LessThanCacheDuration).ConfigureAwait(false);

        // Act - put new cache value and get cached value
        var newCacheValue = "Message-1";
        functionMock.Setup(x => x()).Returns(Task.FromResult(newCacheValue));
        var cachedValue = await cache.GetOrAddWithAbsoluteExpirationAsync(cacheName, functionMock.Object, CacheDuration).ConfigureAwait(false);

        // Assert that the cached value still holds the initial value because the cache has not expired
        Assert.That(cachedValue, Is.EqualTo(initialCacheValue));
        functionMock.Verify(x => x(), Times.Never);
    }

    [Test]
    public async Task RecalculatesCachedValueAfterTimeHasPassed_WhenAbsoluteExpirationIsSpecifiedAsync()
    {
        // Arrange - Setup ApplicationCache, Mock & put initial cache value that will expire in 100 milliseconds
        using var cache = new ApplicationCache();
        const string cacheName = "CacheTests-AbsoluteExp2";

        const string initialCacheValue = "Message-0";
        var functionMock = new Mock<Func<Task<string>>>();
        functionMock.Setup(x => x()).Returns(Task.FromResult(initialCacheValue));
        await cache.GetOrAddWithAbsoluteExpirationAsync(cacheName, functionMock.Object, CacheDuration).ConfigureAwait(false);

        functionMock.Invocations.Clear();
        await Task.Delay(GreaterThanCacheDuration).ConfigureAwait(false); // delay for 110 milliseconds to make the cache to expire

        // Act - put new cache value and get cached value
        var newCacheValue = "Message-1";
        functionMock.Setup(x => x()).Returns(Task.FromResult(newCacheValue));
        var cachedValue = await cache.GetOrAddWithAbsoluteExpirationAsync(cacheName, functionMock.Object, CacheDuration).ConfigureAwait(false);

        // Assert that the cached value contains the new value because the cache has expired
        Assert.That(cachedValue, Is.EqualTo(newCacheValue));
        functionMock.Verify(x => x(), Times.Once);
    }

    [Test]
    public async Task RecalculatesCachedValueAfterTimeHasPassed_DespiteOfCachedValueWasRetrieved_WhenAbsoluteExpirationIsSpecifiedAsync()
    {
        // Arrange - Setup ApplicationCache, Mock & put initial cache value that will expire in 100 milliseconds
        using var cache = new ApplicationCache();
        const string cacheName = "CacheTests-AbsoluteExp3";

        const string initialCacheValue = "Message-0";
        var functionMock = new Mock<Func<Task<string>>>();
        functionMock.Setup(x => x()).Returns(Task.FromResult(initialCacheValue));
        await cache.GetOrAddWithAbsoluteExpirationAsync(cacheName, functionMock.Object, CacheDuration).ConfigureAwait(false);

        await Task.Delay(LessThanCacheDuration).ConfigureAwait(false);

        await cache.GetOrAddWithAbsoluteExpirationAsync(cacheName, functionMock.Object, CacheDuration).ConfigureAwait(false);

        functionMock.Invocations.Clear();
        await Task.Delay(AdditionalLessThanCacheDuration).ConfigureAwait(false); // delay for another 70 milliseconds so the total delay is 120 (50 + 70) milliseconds since the 1st cache call

        // Act - put new cache value and get the cached value
        var newCacheValue = "Message-1";
        functionMock.Setup(x => x()).Returns(Task.FromResult(newCacheValue));
        var cachedValue = await cache.GetOrAddWithAbsoluteExpirationAsync(cacheName, functionMock.Object, CacheDuration).ConfigureAwait(false);

        // Assert that the cached value has new cache value because the cache has expired since the 1st cache call
        Assert.That(cachedValue, Is.EqualTo(newCacheValue));
        functionMock.Verify(x => x(), Times.Once);
    }
}
