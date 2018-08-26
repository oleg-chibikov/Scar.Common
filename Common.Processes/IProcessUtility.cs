using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Scar.Common.Events;

namespace Scar.Common.Processes
{
    public interface IProcessUtility
    {
        event EventHandler<EventArgs<string>> ProcessErrorFired;
        event EventHandler<EventArgs<string>> ProcessMessageFired;

        [NotNull]
        Task<ProcessResult> ExecuteCommandAsync(
            [NotNull] string commandPath,
            [CanBeNull] string arguments,
            CancellationToken token,
            [CanBeNull] TimeSpan? timeout = null,
            [CanBeNull] string workingDirectory = null);

        [NotNull]
        Task TaskKillAsync([NotNull] string processName, CancellationToken token);
    }
}