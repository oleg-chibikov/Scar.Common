using System;
using Scar.Common.ApplicationLifetime.Contracts;

namespace Scar.Common.WebApi.Startup
{
    class ConsoleApplicationTerminator : IApplicationTerminator
    {
        public void Terminate()
        {
            Environment.Exit(-1);
        }
    }
}
