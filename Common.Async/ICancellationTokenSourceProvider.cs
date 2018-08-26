using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Scar.Common.Async
{
    public interface ICancellationTokenSourceProvider
    {
        CancellationToken Token { get; }

        [NotNull]
        Task CurrentTask { get; }

        void Cancel();

        [NotNull]
        Task ExecuteAsyncOperation([NotNull] Func<CancellationToken, Task> func, bool cancelCurrent = true);

        [NotNull]
        Task StartNewTask([NotNull] Action<CancellationToken> action, bool cancelCurrent = true);

        bool CheckCompleted();
    }
}