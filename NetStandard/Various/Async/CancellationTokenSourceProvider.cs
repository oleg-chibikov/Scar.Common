using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scar.Common.Async
{
    public sealed class CancellationTokenSourceProvider : IDisposable, ICancellationTokenSourceProvider
    {
        CancellationTokenSource _cancellationTokenSource = new ();

        public Task CurrentTask { get; private set; } = Task.CompletedTask;

        public CancellationToken Token => _cancellationTokenSource.Token;

        public async Task StartNewTaskAsync(Action<CancellationToken> action, bool cancelCurrent)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            await ExecuteOperationAsync(cancellationToken => Task.Run(() => action(cancellationToken), cancellationToken), cancelCurrent).ConfigureAwait(false);
        }

        public bool CheckCompleted()
        {
            return CurrentTask.IsCompleted;
        }

        public async Task ExecuteOperationAsync(Func<CancellationToken, Task> func, bool cancelCurrent)
        {
            _ = func ?? throw new ArgumentNullException(nameof(func));
            if (!cancelCurrent && !CheckCompleted())
            {
                return;
            }

            try
            {
                var cancellationToken = ResetToken();
                CurrentTask = func(cancellationToken);
                await CurrentTask.ConfigureAwait(false);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public CancellationToken ResetTokenIfNeeded()
        {
            return _cancellationTokenSource.IsCancellationRequested ? ResetToken() : Token;
        }

        public CancellationToken ResetToken()
        {
            var newCts = new CancellationTokenSource();
            var oldCts = Interlocked.Exchange(ref _cancellationTokenSource, newCts);
            oldCts?.Cancel();
            return newCts.Token;
        }

        public void Cancel()
        {
            try
            {
                _cancellationTokenSource.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
        }
    }
}
