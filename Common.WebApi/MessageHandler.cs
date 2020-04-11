using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Scar.Common.WebApi
{
    public abstract class MessageHandler : DelegatingHandler
    {
        protected abstract Task IncomingMessageAsync(HttpRequestMessage request, string requestInfo, string? message);

        protected abstract Task OutgoingMessageAsync(
            HttpRequestMessage request,
            string requestInfo,
            string? message,
            HttpStatusCode responseStatusCode);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.LocalPath.Contains("swagger"))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            Trace.CorrelationManager.ActivityId = Guid.NewGuid();
            var requestInfo = $"{request.Method} {request.RequestUri}";
            var requestMessage = request.Content == null ? null : await request.Content.ReadAsStringAsync();
            await IncomingMessageAsync(request, requestInfo, requestMessage);
            var response = await base.SendAsync(request, cancellationToken);
            string? responseMessage = null;
            if (response.Content != null)
            {
                responseMessage = await response.Content.ReadAsStringAsync();
            }

            await OutgoingMessageAsync(request, requestInfo, responseMessage, response.StatusCode);
            return response;
        }
    }
}
