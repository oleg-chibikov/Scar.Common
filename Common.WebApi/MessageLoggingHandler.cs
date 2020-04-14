using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;

namespace Scar.Common.WebApi
{
    public sealed class MessageLoggingHandler : MessageHandler
    {
        protected override async Task IncomingMessageAsync(HttpRequestMessage request, string requestInfo, string? message)
        {
            var logger = (ILog)request.GetDependencyScope().GetService(typeof(ILog));
            await Task.Run(() => logger.Debug($"Request: {GetMessage(requestInfo, message)}"));
        }

        protected override async Task OutgoingMessageAsync(HttpRequestMessage request, string requestInfo, string? message, HttpStatusCode responseStatusCode)
        {
            var logger = (ILog)request.GetDependencyScope().GetService(typeof(ILog));
            await Task.Run(() => logger.Debug($"Response: {GetMessage(requestInfo, message)}: ({(int)responseStatusCode} {responseStatusCode})"));
        }

        static string GetMessage(string requestInfo, string? message)
        {
            var stringBuilder = new StringBuilder(requestInfo);
            if ((message != null) && !string.IsNullOrEmpty(message))
            {
                stringBuilder.Append(": ");
                stringBuilder.Append(message.Trim());
            }

            return stringBuilder.ToString();
        }
    }
}
