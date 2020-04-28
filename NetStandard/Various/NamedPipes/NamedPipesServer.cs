using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Scar.Common.NamedPipes
{
    public sealed class NamedPipesServer<T> : INamedPipesServer<T>
    {
        const int ReadBufferSize = 255;

        readonly ILogger _logger;

        NamedPipeServerStream _pipeServer;

        public NamedPipesServer(ILogger<NamedPipesServer<T>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            try
            {
                _pipeServer = CreateListener();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Cannot create listener");
                throw;
            }
        }

        public event EventHandler<MessageEventArgs<T>>? MessageReceived;

        [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "This property is OK")]
        public static string PipeName => "NamedPipe_" + typeof(T).Name;

        public void Dispose()
        {
            _pipeServer.Dispose();
            _logger.LogDebug("Finished listening");
        }

        NamedPipeServerStream CreateListener()
        {
            _logger.LogDebug("Listening for the new message...");

            // Create the new async pipe
            var stream = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

            // Wait for a connection
            stream.BeginWaitForConnection(WaitForConnectionCallBack, _pipeServer);
            return stream;
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "General catch, don't know underlying exceptions")]
        void WaitForConnectionCallBack(IAsyncResult asyncResult)
        {
            try
            {
                // End waiting for the connection
                try
                {
                    _pipeServer.EndWaitForConnection(asyncResult);
                }
                catch (ObjectDisposedException)
                {
                    // Normal termination
                    return;
                }

                _logger.LogDebug("Receiving message from client...");
                var bf = new BinaryFormatter();
                T message;
                using (var ms = new MemoryStream())
                {
                    do
                    {
                        var messageBuffer = new byte[ReadBufferSize];
                        var read = _pipeServer.Read(messageBuffer, 0, messageBuffer.Length);
                        ms.Write(messageBuffer, 0, read);
                    }
                    while (!_pipeServer.IsMessageComplete);

                    ms.Seek(0, SeekOrigin.Begin);
                    using var decompressingStream = new DeflateStream(ms, CompressionMode.Decompress);
                    using var decompressedStream = decompressingStream.CreateMemoryStream();
                    message = (T)bf.Deserialize(decompressedStream);
                }

                _pipeServer.Close();
                _pipeServer.Dispose();
                _logger.LogDebug($"Message {message} is received");
                var handler = Volatile.Read(ref MessageReceived);
                if (handler == null)
                {
                    _logger.LogDebug("No handler is registered");
                }
                else
                {
                    _logger.LogDebug("Handling message");
                    handler.Invoke(this, new MessageEventArgs<T>(message));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Wait for connection callback error");
            }

            _pipeServer = CreateListener();
        }
    }
}
