using System;

namespace Scar.Common.Console.Startup;

interface IExitSignal
{
    event EventHandler? Exit;
}
