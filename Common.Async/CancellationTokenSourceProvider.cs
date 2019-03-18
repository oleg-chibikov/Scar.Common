using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scar.Common.Async
{
    public sealed class CancellationTokenSourceProvider : IDisposable, ICancellationTokenSourceProvider
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public Task CurrentTask { get; private set; } = Task.CompletedTask;

        public CancellationToken Token => _cancellationTokenSource.Token;

        public async Task StartNewTask(Action<CancellationToken> action, bool cancelCurrent)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            await ExecuteAsyncOperation(token => Task.Run(() => action(token), token), cancelCurrent).ConfigureAwait(false);
        }

        public bool CheckCompleted()
        {
            return CurrentTask.IsCompleted;
        }

        public async Task ExecuteAsyncOperation(Func<CancellationToken, Task> func, bool cancelCurrent)
        {
            _ = func ?? throw new ArgumentNullException(nameof(func));
            if (!cancelCurrent && !CheckCompleted())
            {
                return;
            }

            try
            {
                var newCts = new CancellationTokenSource();
                var oldCts = Interlocked.Exchange(ref _cancellationTokenSource, newCts);
                oldCts?.Cancel();
                var token = newCts.Token;
                CurrentTask = func(token);
                await CurrentTask.ConfigureAwait(false);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
        }
    }
}