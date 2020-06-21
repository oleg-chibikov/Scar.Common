using System;
using Microsoft.Extensions.Logging;

namespace Scar.Common.WebApi.Startup.Launcher
{
    public sealed class Dependency : IDisposable
    {
        readonly ILogger<Dependency> _logger;

        public Dependency(ILogger<Dependency> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Method()
        {
            _logger.LogInformation("Method is called");
        }

        public void Dispose()
        {
            _logger.LogInformation("Dependency is disposed");
        }
    }
}
