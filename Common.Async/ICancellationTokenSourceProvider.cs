using System;
using System.Threading;
using System.Threading.Tasks;

namespace Scar.Common.Async
{
    public interface ICancellationTokenSourceProvider
    {
        CancellationToken Token { get; }
        Task CurrentTask { get; }
        void Cancel();
        CancellationToken ResetTokenIfNeeded();
        CancellationToken ResetToken();
        Task ExecuteAsyncOperation(Func<CancellationToken, Task> func, bool cancelCurrent = true);
        Task StartNewTask(Action<CancellationToken> action, bool cancelCurrent = true);
        bool CheckCompleted();
    }
}