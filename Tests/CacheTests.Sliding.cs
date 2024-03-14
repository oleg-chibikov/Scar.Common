using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Scar.Common.Caching;

namespace Scar.Common.Tests.Tests;

public partial class CacheTests
{
    [Test]
    public async Task UsesCachedValueUntilTimeHasPassed_WhenSlidingExpirationIsSpecifiedAsync()
    {
        // Arrange - Setup ApplicationCache, Mock & put initial cache value that will expire in 100 milliseconds
        using var cache = new ApplicationCache();
        const string cacheName = "CacheTests-SlidingExp1";

        var functionMock = new Mock<Func<Task<string>>>();
        const string initialCacheValue = "Message-0";
        functionMock.Setup(x => x()).Returns(Task.FromResult(initialCacheValue));
        await cache.GetOrAddWithSlidingExpirationAsync(cacheName, functionMock.Object, CacheDuration).ConfigureAwait(false);

        functionMock.Invocations.Clear();
        await Task.Delay(LessThanCacheDuration).ConfigureAwait(false);

        // Act - put new cache value and get cached value
        var newCacheValue = "Message-1";
        functionMock.Setup(x => x()).Returns(Task.FromResult(newCacheValue));
        var cachedValue = await cache.GetOrAddWithSlidingExpirationAsync(cacheName, functionMock.Object, CacheDuration).ConfigureAwait(false);

        // Assert that the cached value still holds the initial value because the cache has not expired
        Assert.That(cachedValue, Is.EqualTo(initialCacheValue));
        functionMock.Verify(x => x(), Times.Never);
    }

    [Test]
    public async Task RecalculatesCachedValueAfterTimeHasPassed_WhenSlidingExpirationIsSpecifiedAsync()
    {
        // Arrange - Setup ApplicationCache, Mock & put initial cache value that will expire in 100 milliseconds
        using var cache = new ApplicationCache();
        const string cacheName = "CacheTests-SlidingExp2";

        // Arrange - put initial cache value that will expire in 100 milliseconds
        var functionMock = new Mock<Func<Task<string>>>();
        const string initialCacheValue = "Message-0";
        functionMock.Setup(x => x()).Returns(Task.FromResult(initialCacheValue));
        await cache.GetOrAddWithSlidingExpirationAsync(cacheName, functionMock.Object, CacheDuration).ConfigureAwait(false);

        functionMock.Invocations.Clear();
        await Task.Delay(GreaterThanCacheDuration).ConfigureAwait(false);

        // Act - put new cache value and get cached value
        var newCacheValue = "Message-1";
        functionMock.Setup(x => x()).Returns(Task.FromResult(newCacheValue));
        var cachedValue = await cache.GetOrAddWithSlidingExpirationAsync(cacheName, functionMock.Object, CacheDuration).ConfigureAwait(false);

        // Assert that the cached value has the new value because the cache has expired
        Assert.That(cachedValue, Is.EqualTo(newCacheValue));
        functionMock.Verify(x => x(), Times.Once);
    }

    [Test]
    public async Task UsesCachedValueAfterTimeHasPassed_AndCachedValueWasRetrieved_WhenSlidingExpirationIsSpecifiedAsync()
    {
        // Arrange - Setup ApplicationCache, Mock & put initial cache value that will expire in 100 milliseconds
        using var cache = new ApplicationCache();
        const string cacheName = "CacheTests-SlidingExp3";

        // Arrange - put initial cache value that will expire in 100 milliseconds
        var functionMock = new Mock<Func<Task<string>>>();
        const string initialCacheValue = "Message-0";
        functionMock.Setup(x => x()).Returns(Task.FromResult(initialCacheValue));
        await cache.GetOrAddWithSlidingExpirationAsync(cacheName, functionMock.Object, CacheDuration).ConfigureAwait(false);

        await Task.Delay(LessThanCacheDuration).ConfigureAwait(false);

        await cache.GetOrAddWithSlidingExpirationAsync(cacheName, functionMock.Object, CacheDuration).ConfigureAwait(false);

        functionMock.Invocations.Clear();
        await Task.Delay(AdditionalLessThanCacheDuration).ConfigureAwait(false); // delay for another 70 milliseconds so the total delay is 120 (50 + 70) milliseconds since the 1st cache call

        // Act - put new cache value and get the cached value
        var newCacheValue = "Message-1";
        functionMock.Setup(x => x()).Returns(Task.FromResult(newCacheValue));
        var cachedValue = await cache.GetOrAddWithSlidingExpirationAsync(cacheName, functionMock.Object, CacheDuration).ConfigureAwait(false);

        // Assert that the cached value should still holds the initial value because the limit was slided thus cache has not expired
        Assert.That(cachedValue, Is.EqualTo(initialCacheValue));
        functionMock.Verify(x => x(), Times.Never);
    }
}
