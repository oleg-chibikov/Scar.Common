using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using Common.Logging;
using Scar.Common.Exceptions;

namespace Scar.Common.WebApi
{
    public sealed class LocalizableExceptionLogger : IExceptionLogger
    {
        public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            var logger = (ILog)context.Request.GetDependencyScope().GetService(typeof(ILog));
            if (context.Exception is LocalizableException localizableException)
            {
                logger.Warn(localizableException.LocalizedMessage);
            }
            else
            {
                logger.Error(context.Exception);
            }

            return Task.FromResult(0);
        }
    }
}
