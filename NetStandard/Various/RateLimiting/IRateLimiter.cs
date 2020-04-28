using System;
using System.Threading.Tasks;

namespace Scar.Common.RateLimiting
{
    public interface IRateLimiter
    {
        Task DebounceAsync<T>(TimeSpan interval, Action<T> action, T param);

        Task DebounceAsync(TimeSpan interval, Action action);

        Task ThrottleAsync<T>(TimeSpan interval, Action<T> action, T param, bool skipImmediateEvent = false, bool useFirstEvent = false);

        Task ThrottleAsync(TimeSpan interval, Action action, bool skipImmediate = false, bool skipLast = false);
    }
}
