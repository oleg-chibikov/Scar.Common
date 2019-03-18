using System;

namespace Scar.Common
{
    public interface IRateLimiter
    {
        void Debounce<T>(TimeSpan interval, Action<T> action, T param);

        void Debounce(TimeSpan interval, Action action);

        void Throttle<T>(TimeSpan interval, Action<T> action, T param, bool skipImmediateEvent = false, bool useFirstEvent = false);

        void Throttle(TimeSpan interval, Action action, bool skipImmediate = false, bool skipLast = false);
    }
}