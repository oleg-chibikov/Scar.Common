using System;
using System.Threading;
using System.Threading.Tasks;
using Scar.Common.View.Contracts;

namespace Scar.Common.View.WindowCreation;

public class WindowFactory<TWindow>(IScopedWindowProvider scopedWindowProvider, IAsyncWindowDisplayer windowDisplayer)
    : IWindowFactory<TWindow>, IDisposable
    where TWindow : class, IDisplayable
{
    readonly IScopedWindowProvider _scopedWindowProvider = scopedWindowProvider ?? throw new ArgumentNullException(nameof(scopedWindowProvider));
    readonly IAsyncWindowDisplayer _windowDisplayer = windowDisplayer ?? throw new ArgumentNullException(nameof(windowDisplayer));
    readonly SemaphoreSlim _semaphore = new (1, 1);
    TWindow? _currentWindow;
    bool _disposedValue;

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

    public async Task<Action<Action<TWindow>>> ShowWindowAsync(CancellationToken cancellationToken)
    {
        return await ShowWindowAsync<object>(default!, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Action<Action<TWindow>>> ShowWindowAsync<TParam>(TParam param, CancellationToken cancellationToken)
    {
        return await _windowDisplayer.DisplayWindowAsync(
                async () =>
                {
                    var window = await GetOrCreateWindowAsync(param, cancellationToken).ConfigureAwait(false);
                    window.Restore();
                    return window;
                }, cancellationToken)
            .ConfigureAwait(false);
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

    async Task<TWindow> GetOrCreateWindowAsync<TParam>(TParam param, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        var window = _currentWindow;
        if (window == null)
        {
            window = await CreateWindowAsync(param, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            _semaphore.Release();
        }

        return window;
    }

    async Task<TWindow> CreateWindowAsync<TParam>(TParam param, CancellationToken cancellationToken)
    {
        try
        {
            var window = await _scopedWindowProvider.GetScopedWindowAsync<TWindow, TParam>(param, cancellationToken).ConfigureAwait(false);

            window.Closed += WindowClosed;
            window.ContentRendered += ContentRendered;
            return window;

            void WindowClosed(object? sender, EventArgs args)
            {
                _currentWindow = default;
                _ = window ?? throw new InvalidOperationException("Window is null");
                window.Closed -= WindowClosed;
            }

            void ContentRendered(object? sender, EventArgs e)
            {
                _ = window ?? throw new InvalidOperationException("Window is null");
                window.ContentRendered -= ContentRendered;
                _currentWindow = window;
                _semaphore.Release();
            }
        }
        catch
        {
            _semaphore.Release();
            throw;
        }
    }
}
