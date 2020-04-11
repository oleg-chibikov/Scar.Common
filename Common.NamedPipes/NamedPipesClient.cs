using System;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using Common.Logging;

namespace Scar.Common.NamedPipes
{
    public sealed class NamedPipesClient<T> : INamedPipesClient<T>
    {
        private readonly ILog _logger;

        public NamedPipesClient(ILog logger)
        {
            _logger = logger;
        }

        public void SendMessage(T message, int timeout)
        {
            try
            {
                _logger.Debug($"Sending message {message}...");
                var pipeClient = new NamedPipeClientStream(".", NamedPipesServer<T>.PipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                pipeClient.Connect(timeout);

                _logger.Debug("Connection established");

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
                _logger.Debug("Message is sent");
            }
            catch (TimeoutException e)
            {
                _logger.Warn(e);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }
    }
}
