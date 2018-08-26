using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using JetBrains.Annotations;
using Scar.Common.WPF.View.Contracts;

namespace Scar.Common.WPF.View
{
    public class WindowFactory<TWindow> : IWindowFactory<TWindow>
        where TWindow : class, IWindow
    {
        [NotNull]
        private readonly IScopedWindowProvider _scopedWindowProvider;

        [NotNull]
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        [CanBeNull]
        private TWindow _currentWindow;

        public WindowFactory([NotNull] IScopedWindowProvider scopedWindowProvider)
        {
            _scopedWindowProvider = scopedWindowProvider ?? throw new ArgumentNullException(nameof(scopedWindowProvider));
        }

        public async Task<TWindow> GetWindowAsync(CancellationToken cancellationToken)
        {
            return await GetWindowIfExistsAsync(cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException("Current window does not exist");
        }

        public async Task<TWindow> GetWindowIfExistsAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            var window = _currentWindow;
            _semaphore.Release();
            return window;
        }

        public async Task<TWindow> ShowWindowAsync(CancellationToken cancellationToken)
        {
            return await ShowWindowAsync<TWindow, object>(null, cancellationToken, null).ConfigureAwait(false);
        }

        public async Task<TWindow> ShowWindowAsync<TParam>(CancellationToken cancellationToken, TParam param)
        {
            return await ShowWindowAsync<IWindow, TParam>(null, cancellationToken, param).ConfigureAwait(false);
        }

        public async Task<TWindow> ShowWindowAsync<TSplashWindow>(IWindowFactory<TSplashWindow> splashWindowFactory, CancellationToken cancellationToken)
            where TSplashWindow : class, IWindow
        {
            return await ShowWindowAsync<TSplashWindow, object>(splashWindowFactory, cancellationToken, null).ConfigureAwait(false);
        }

        public async Task<TWindow> ShowWindowAsync<TSplashWindow, TParam>(IWindowFactory<TSplashWindow> splashWindowFactory, CancellationToken cancellationToken, TParam param)
            where TSplashWindow : class, IWindow
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            var window = _currentWindow;
            if (window == null)
            {
                try
                {
                    TSplashWindow splashWindow = null;
                    if (splashWindowFactory != null)
                    {
                        splashWindow = await splashWindowFactory.ShowWindowAsync(cancellationToken).ConfigureAwait(false);
                    }

                    window = await _scopedWindowProvider.GetScopedWindowAsync<TWindow, TParam>(param, cancellationToken).ConfigureAwait(false);

                    void WindowClosed(object sender, EventArgs args)
                    {
                        _currentWindow = default(TWindow);
                        window.Closed -= WindowClosed;
                    }

                    void WindowLoaded(object s, RoutedEventArgs e)
                    {
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
    }
}