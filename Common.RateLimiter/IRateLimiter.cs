using System;
using JetBrains.Annotations;

namespace Scar.Common
{
    public interface IRateLimiter
    {
        void Debounce<T>(TimeSpan interval, [NotNull] Action<T> action, [CanBeNull] T param);

        void Debounce(TimeSpan interval, [NotNull] Action action);

        void Throttle<T>(TimeSpan interval, [NotNull] Action<T> action, [CanBeNull] T param, bool skipImmediateEvent = false, bool useFirstEvent = false);

        void Throttle(TimeSpan interval, [NotNull] Action action, bool skipImmediate = false, bool skipLast = false);
    }
}