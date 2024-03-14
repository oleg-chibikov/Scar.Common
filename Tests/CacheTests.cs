using System;

namespace Scar.Common.Tests.Tests;

public partial class CacheTests
{
    static readonly TimeSpan CacheDuration = TimeSpan.FromMilliseconds(1000);
    static readonly TimeSpan LessThanCacheDuration = TimeSpan.FromMilliseconds(500);
    static readonly TimeSpan AdditionalLessThanCacheDuration = TimeSpan.FromMilliseconds(700);
    static readonly TimeSpan GreaterThanCacheDuration = TimeSpan.FromMilliseconds(1100);
}