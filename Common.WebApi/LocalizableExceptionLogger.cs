using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using Common.Logging;
using JetBrains.Annotations;
using Scar.Common.Exceptions;

namespace Scar.Common.WebApi
{
    public sealed class LocalizableExceptionLogger : IExceptionLogger
    {
        [NotNull]
        public Task LogAsync([NotNull] ExceptionLoggerContext context, CancellationToken cancellationToken)
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