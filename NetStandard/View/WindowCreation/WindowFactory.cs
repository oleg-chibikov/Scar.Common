using System;
using System.Threading;
using System.Threading.Tasks;
using Scar.Common.View.Contracts;

namespace Scar.Common.View.WindowCreation
{
    public class WindowFactory<TWindow> : IWindowFactory<TWindow>, IDisposable
        where TWindow : class, IDisplayable
    {
        readonly IScopedWindowProvider _scopedWindowProvider;
        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        TWindow? _currentWindow;
        bool _disposedValue;

        public WindowFactory(IScopedWindowProvider scopedWindowProvider)
        {
            _scopedWindowProvider = scopedWindowProvider ?? throw new ArgumentNullException(nameof(scopedWindowProvider));
        }

        public async Task<TWindow> GetWindowAsync(CancellationToken cancellationToken)
        {
            return await GetWindowIfExistsAsync(cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException("Current window does not exist");
        }

        public async Task<TWindow?> GetWindowIfExistsAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            var window = _currentWindow;
            _semaphore.Release();
            return window;
        }

        public async Task<TWindow> ShowWindowAsync(CancellationToken cancellationToken)
        {
            return await ShowWindowAsync<TWindow, object>(null, default!, cancellationToken).ConfigureAwait(false);
        }

        public async Task<TWindow> ShowWindowAsync<TParam>(TParam param, CancellationToken cancellationToken)
        {
            return await ShowWindowAsync<IDisplayable, TParam>(null, param, cancellationToken).ConfigureAwait(false);
        }

        public async Task<TWindow> ShowWindowAsync<TSplashWindow>(IWindowFactory<TSplashWindow>? splashWindowFactory, CancellationToken cancellationToken)
            where TSplashWindow : class, IDisplayable
        {
            return await ShowWindowAsync<TSplashWindow, object>(splashWindowFactory, default!, cancellationToken).ConfigureAwait(false);
        }

        public async Task<TWindow> ShowWindowAsync<TSplashWindow, TParam>(IWindowFactory<TSplashWindow>? splashWindowFactory, TParam param, CancellationToken cancellationToken)
            where TSplashWindow : class, IDisplayable
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            var window = _currentWindow;
            if (window == null)
            {
                try
                {
                    TSplashWindow? splashWindow = null;
                    if (splashWindowFactory != null)
                    {
                        splashWindow = await splashWindowFactory.ShowWindowAsync(cancellationToken).ConfigureAwait(false);
                    }

                    window = await _scopedWindowProvider.GetScopedWindowAsync<TWindow, TParam>(param, cancellationToken).ConfigureAwait(false);

                    void WindowClosed(object sender, EventArgs args)
                    {
                        _currentWindow = default;
                        _ = window ?? throw new InvalidOperationException("Window is null");
                        window.Closed -= WindowClosed;
                    }

                    void WindowLoaded(object s, EventArgs e)
                    {
                        _ = window ?? throw new InvalidOperationException("Window is null");
                        window.Loaded -= WindowLoaded;
                        splashWindow?.Close();
                        _currentWindow = window;
                        _semaphore.Release();
                    }

                    window.Closed += WindowClosed;
                    window.Loaded += WindowLoaded;
                }
                catch
                {
                    _semaphore.Release();
                    throw;
                }
            }
            else
            {
                _semaphore.Release();
            }

            window.Restore();
            return window;
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
                    _semaphore.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
