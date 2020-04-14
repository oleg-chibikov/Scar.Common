using System;
using System.Threading.Tasks;

namespace Scar.Common
{
    public static class ActionExtensions
    {
        public static async Task<TReturn> RunFuncWithSeveralAttemptsAsync<TReturn>(
            this Func<AttemptInfo, TReturn> func,
            Func<AttemptInfo, Exception, bool>? canRetryAtException = null,
            TimeSpan? delay = null,
            int maxAttempts = 3,
            bool throwOnAttemptLimit = false,
            bool configureAwait = false)
        {
            var attempt = 0;
            while (true)
            {
                var attemptInfo = new AttemptInfo(attempt, maxAttempts);
                try
                {
                    return func(attemptInfo);
                }
                catch (Exception ex)
                {
                    if ((canRetryAtException?.Invoke(attemptInfo, ex) != true) && (attempt++ < maxAttempts))
                    {
                        if (delay.HasValue)
                        {
                            await Task.Delay(delay.Value).ConfigureAwait(configureAwait);
                        }
                    }
                    else if (throwOnAttemptLimit)
                    {
                        throw;
                    }
                    else
                    {
                        return default!;
                    }
                }
            }
        }

        public static async Task<TReturn> RunTaskWithSeveralAttemptsAsync<TReturn>(
            this Func<AttemptInfo, Task<TReturn>> taskFactory,
            Func<AttemptInfo, Exception, bool>? canRetryAtException = null,
            TimeSpan? delay = null,
            int maxAttempts = 3,
            bool throwOnAttemptLimit = false,
            bool configureAwait = false)
        {
            var attempt = 0;
            while (true)
            {
                var attemptInfo = new AttemptInfo(attempt, maxAttempts);
                try
                {
                    return await taskFactory(attemptInfo).ConfigureAwait(configureAwait);
                }
                catch (Exception ex)
                {
                    if ((canRetryAtException?.Invoke(attemptInfo, ex) != true) && (attempt++ < maxAttempts))
                    {
                        if (delay.HasValue)
                        {
                            await Task.Delay(delay.Value).ConfigureAwait(configureAwait);
                        }
                    }
                    else if (throwOnAttemptLimit)
                    {
                        throw;
                    }
                    else
                    {
                        return default!;
                    }
                }
            }
        }
    }
}
