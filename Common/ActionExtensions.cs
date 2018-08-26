using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Scar.Common
{
    public static class ActionExtensions
    {
        [CanBeNull]
        public static async Task<TReturn> RunFuncWithSeveralAttemptsAsync<TReturn>(
            [NotNull] this Func<AttemptInfo, TReturn> func,
            [CanBeNull] Func<AttemptInfo, Exception, bool> canRetryAtException = null,
            [CanBeNull] TimeSpan? delay = null,
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
                    if (canRetryAtException?.Invoke(attemptInfo, ex) != true && attempt++ < maxAttempts)
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
                        return default(TReturn);
                    }
                }
            }
        }

        [CanBeNull]
        public static async Task<TReturn> RunTaskWithSeveralAttemptsAsync<TReturn>(
            [NotNull] this Func<AttemptInfo, Task<TReturn>> taskFactory,
            [CanBeNull] Func<AttemptInfo, Exception, bool> canRetryAtException = null,
            [CanBeNull] TimeSpan? delay = null,
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
                    if (canRetryAtException?.Invoke(attemptInfo, ex) != true && attempt++ < maxAttempts)
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
                        return default(TReturn);
                    }
                }
            }
        }
    }
}