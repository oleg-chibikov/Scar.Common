using System;
using System.Threading.Tasks;
using Mono.Unix;

namespace Scar.Common.Console.Startup;

sealed class UnixExitSignal : IExitSignal
{
    readonly UnixSignal[] _signals = {
        new(Mono.Unix.Native.Signum.SIGTERM),
        new(Mono.Unix.Native.Signum.SIGINT),
        new(Mono.Unix.Native.Signum.SIGUSR1)
    };

    public UnixExitSignal()
    {
        Task.Run(() =>
        {
            // blocking call to wait for any kill signal
            _ = UnixSignal.WaitAny(_signals, -1);

            Exit?.Invoke(null, EventArgs.Empty);

        });
    }

    public event EventHandler? Exit;
}
