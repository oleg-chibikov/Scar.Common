using System;
using System.Runtime.InteropServices;

namespace Scar.Common.Console.Startup;

sealed class WinExitSignal : IExitSignal
{
    /// <summary>
    /// Need this as a member variable to avoid it being garbage collected.
    /// </summary>
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    readonly HandlerRoutine _handlerRoutine;

    public WinExitSignal()
    {
        _handlerRoutine = ConsoleCtrlCheck;

        SetConsoleCtrlHandler(_handlerRoutine, true);

    }

    // A delegate type to be used as the handler routine
    // for SetConsoleCtrlHandler.
    public delegate bool HandlerRoutine(CtrlTypes ctrlType);

    public event EventHandler? Exit;

    // An enumerated type for the control messages
    // sent to the handler routine.
    public enum CtrlTypes
    {
        CtrlCEvent = 0,
        CtrlBreakEvent,
        CtrlCloseEvent,
        CtrlLogoffEvent = 5,
        CtrlShutdownEvent
    }

    [DllImport("Kernel32")]
#pragma warning disable CA5392 // Use DefaultDllImportSearchPaths attribute for P/Invokes
    static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);
#pragma warning restore CA5392 // Use DefaultDllImportSearchPaths attribute for P/Invokes

    /// <summary>
    /// Handle the ctrl types
    /// </summary>
    bool ConsoleCtrlCheck(CtrlTypes ctrlType)
    {
        switch (ctrlType)
        {
            case CtrlTypes.CtrlCEvent:
            case CtrlTypes.CtrlBreakEvent:
            case CtrlTypes.CtrlCloseEvent:
            case CtrlTypes.CtrlLogoffEvent:
            case CtrlTypes.CtrlShutdownEvent:

                Exit?.Invoke(this, EventArgs.Empty);
                break;
        }

        return true;
    }
}
