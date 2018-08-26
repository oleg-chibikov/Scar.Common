using System;
using System.Threading;
using JetBrains.Annotations;

namespace Scar.Common
{
    public class RateLimiter : IRateLimiter
    {
        [NotNull]
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        [CanBeNull]
        private readonly SynchronizationContext _synchronizationContext;

        [CanBeNull]
        private Timer _timer;

        public RateLimiter([CanBeNull] SynchronizationContext synchronizationContext = null)
        {
            _synchronizationContext = synchronizationContext;
        }

        private DateTime LastExecutionTime { get; set; } = DateTime.MinValue;

        public async void Debounce<T>(TimeSpan interval, Action<T> action, T param)
        {
            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
            _timer?.Dispose();
            _timer = null;
            _timer = new Timer(
                async s =>
                {
                    await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
                    if (_timer == null)
                    {
                        _semaphoreSlim.Release();
                        return;
                    }

                    _timer?.Dispose();
                    _timer = null;
                    _semaphoreSlim.Release();
                    ExecuteAction(action, param);
                },
                null,
                interval,
                TimeSpan.FromMilliseconds(-1));
            _semaphoreSlim.Release();
        }

        public void Debounce(TimeSpan interval, Action action)
        {
            Debounce<object>(interval, x => action(), null);
        }

        public async void Throttle<T>(TimeSpan interval, Action<T> action, T param, bool skipImmediateEvent = false, bool useFirstEvent = false)
        {
            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
            if (!useFirstEvent)
            {
                _timer?.Dispose();
                _timer = null;
            }

            var curTime = DateTime.UtcNow;

            var timePassed = curTime - LastExecutionTime;

            if (timePassed > interval)
            {
                LastExecutionTime = curTime;
                if (!skipImmediateEvent)
                {
                    _semaphoreSlim.Release();
                    ExecuteAction(action, param);
                }
                else
                {
                    SetThrottleTimer(interval, action, param, curTime);
                    _semaphoreSlim.Release();
                }
            }
            else
            {
                if (!useFirstEvent)
                {
                    if (timePassed <= interval)
                    {
                        interval -= timePassed;
                    }

                    SetThrottleTimer(interval, action, param, curTime);
                }

                _semaphoreSlim.Release();
            }
        }

        public void Throttle(TimeSpan interval, Action action, bool skipImmediate = false, bool skipLast = false)
        {
            Throttle<object>(interval, x => action(), null, skipImmediate, skipLast);
        }

        private void ExecuteAction<T>(Action<T> action, T param)
        {
            if (_synchronizationContext != null)
            {
                _synchronizationContext.Send(state => action(param), null);
            }
            else
            {
                action(param);
            }
        }

        private void SetThrottleTimer<T>(TimeSpan interval, Action<T> action, T param, DateTime curTime)
        {
            _timer = new Timer(
                async s =>
                {
                    await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
                    if (_timer == null)
                    {
                        _semaphoreSlim.Release();
                        return;
                    }

                    _timer?.Dispose();
                    _timer = null;
                    LastExecutionTime = curTime;
                    _semaphoreSlim.Release();
                    ExecuteAction(action, param);
                },
                null,
                interval,
                TimeSpan.FromMilliseconds(-1));
        }
    }
}