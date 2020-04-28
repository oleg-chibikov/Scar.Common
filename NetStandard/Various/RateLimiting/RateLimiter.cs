using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scar.Common.RateLimiting
{
    public class RateLimiter : IRateLimiter, IDisposable
    {
        readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        readonly SynchronizationContext? _synchronizationContext;
        Timer? _timer;
        bool _disposedValue;

        public RateLimiter(SynchronizationContext? synchronizationContext = null)
        {
            _synchronizationContext = synchronizationContext;
        }

        DateTime LastExecutionTime { get; set; } = DateTime.MinValue;

        public async Task DebounceAsync<T>(TimeSpan interval, Action<T> action, T param)
        {
            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
            _timer?.Dispose();
            _timer = null;
            _timer = new Timer(
                async s =>
                {
                    await _semaphoreSlim.WaitAsync().ConfigureAwait(true);
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

        public async Task DebounceAsync(TimeSpan interval, Action action)
        {
            await DebounceAsync<object>(interval, x => action(), default!).ConfigureAwait(false);
        }

        public async Task ThrottleAsync<T>(TimeSpan interval, Action<T> action, T param, bool skipImmediateEvent = false, bool useFirstEvent = false)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));

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

        public async Task ThrottleAsync(TimeSpan interval, Action action, bool skipImmediate = false, bool skipLast = false)
        {
            await ThrottleAsync<object>(interval, x => action(), default!, skipImmediate, skipLast).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _semaphoreSlim.Dispose();
                    _timer?.Dispose();
                }

                _disposedValue = true;
            }
        }

        void ExecuteAction<T>(Action<T> action, T param)
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

        void SetThrottleTimer<T>(TimeSpan interval, Action<T> action, T param, DateTime curTime)
        {
            _timer = new Timer(
                async s =>
                {
                    await _semaphoreSlim.WaitAsync().ConfigureAwait(true);
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
