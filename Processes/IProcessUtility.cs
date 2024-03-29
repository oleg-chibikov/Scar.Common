using System;
using System.Threading;
using System.Threading.Tasks;
using Scar.Common.Events;

namespace Scar.Common.Processes;

public interface IProcessUtility
{
    event EventHandler<EventArgs<string>> ProcessErrorFired;

    event EventHandler<EventArgs<string>> ProcessMessageFired;

    Task<ProcessResult> ExecuteCommandAsync(string commandPath, string? arguments, TimeSpan? timeout = null, string? workingDirectory = null, bool ui = false, CancellationToken cancellationToken = default);

    Task TaskKillAsync(string processName, CancellationToken cancellationToken);
}
