using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Extensions.Logging;

namespace Scar.Common.NamedPipes
{
    public sealed class NamedPipesClient<T> : INamedPipesClient<T>
    {
        readonly ILogger _logger;

        public NamedPipesClient(ILogger logger)
        {
            _logger = logger;
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Don't know underlying exceptions")]
        public void SendMessage(T message, int timeout)
        {
            try
            {
                _logger.LogDebug($"Sending message {message}...");
                var pipeClient = new NamedPipeClientStream(".", NamedPipesServer<T>.PipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                pipeClient.Connect(timeout);

                _logger.LogDebug("Connection established");

                var bf = new BinaryFormatter();
                byte[] buffer;
                using (var ms = new MemoryStream())
                {
                    bf.Serialize(ms, message);
                    ms.Seek(0, SeekOrigin.Begin);
                    using var compressedStream = new MemoryStream();
                    using (var compressingStream = new DeflateStream(compressedStream, CompressionMode.Compress, true))
                    {
                        ms.CopyBuffered(compressingStream);
                    }

                    buffer = compressedStream.ToArray();
                }

                pipeClient.Write(buffer, 0, buffer.Length);
                pipeClient.Dispose();
                _logger.LogDebug("Message is sent");
            }
            catch (TimeoutException e)
            {
                _logger.LogWarning(e, "Timeout during message sending");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception during message sending");
            }
        }
    }
}
