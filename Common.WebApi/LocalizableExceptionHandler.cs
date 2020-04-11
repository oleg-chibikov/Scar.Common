using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Scar.Common.Exceptions;

namespace Scar.Common.WebApi
{
    public sealed class LocalizableExceptionHandler : IExceptionHandler
    {
        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            var localizableException = context.Exception as LocalizableException;
            var message = localizableException?.LocalizedMessage ?? "An error has occured";
            var status = HttpStatusCode.InternalServerError;
            if (localizableException != null)
            {
                status = localizableException is NotFoundException ? HttpStatusCode.NotFound : HttpStatusCode.BadRequest;
            }

            context.Result = new PlainTextErrorResult(status, context.ExceptionContext.Request, message);
            return Task.FromResult(0);
        }

        private sealed class PlainTextErrorResult : IHttpActionResult
        {
            private readonly HttpStatusCode _statusCode;

            private readonly HttpRequestMessage _request;

            private readonly string _content;

            public PlainTextErrorResult(HttpStatusCode statusCode, HttpRequestMessage request, string content)
            {
                _statusCode = statusCode;
                _request = request ?? throw new ArgumentNullException(nameof(request));
                _content = content ?? throw new ArgumentNullException(nameof(content));
            }

            public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent(_content),
                    RequestMessage = _request
                };
                return await Task.FromResult(response);
            }
        }
    }
}
