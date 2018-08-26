using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using JetBrains.Annotations;
using Scar.Common.Exceptions;

namespace Scar.Common.WebApi
{
    public sealed class LocalizableExceptionHandler : IExceptionHandler
    {
        [NotNull]
        public Task HandleAsync([NotNull] ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            var localizableException = context.Exception as LocalizableException;
            var message = localizableException?.LocalizedMessage ?? "An error has occured";
            var status = HttpStatusCode.InternalServerError;
            if (localizableException != null)
            {
                status = localizableException is NotFoundException ? HttpStatusCode.NotFound : HttpStatusCode.BadRequest;
            }

            context.Result = new PlainTextErrorResult(status)
            {
                Request = context.ExceptionContext.Request,
                Content = message
            };
            return Task.FromResult(0);
        }

        private sealed class PlainTextErrorResult : IHttpActionResult
        {
            private readonly HttpStatusCode _statusCode;

            public PlainTextErrorResult(HttpStatusCode statusCode)
            {
                _statusCode = statusCode;
            }

            public HttpRequestMessage Request { private get; set; }

            public string Content { private get; set; }

            public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent(Content),
                    RequestMessage = Request
                };
                return await Task.FromResult(response);
            }
        }
    }
}