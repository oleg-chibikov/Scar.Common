using System;
using Scar.Common.ApplicationLifetime.Contracts;

namespace Scar.Common.WebApi.Startup;

sealed class ConsoleApplicationTerminator : IApplicationTerminator
{
    public void Terminate()
    {
        Environment.Exit(-1);
    }
}