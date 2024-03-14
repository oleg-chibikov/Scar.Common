using System;
using Scar.Common.ApplicationLifetime.Contracts;

namespace Scar.Common.ApplicationLifetime.Core;

public sealed class ConsoleApplicationTerminator : IApplicationTerminator
{
    public void Terminate()
    {
        Environment.Exit(-1);
    }
}
