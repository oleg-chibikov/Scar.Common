using System;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Common.Logging;

namespace Scar.Common.NamedPipes
{
    public sealed class NamedPipesServer<T> : INamedPipesServer<T>
    {
        private const int ReadBufferSize = 255;

        private readonly ILog _logger;

        private NamedPipeServerStream _pipeServer;

        public NamedPipesServer(ILog logger)
        {
            _logger = logger;
            try
            {
                _pipeServer = CreateListener();
            }
            catch (Exception e)
            {
                logger.Fatal(e.Message);
                throw;
            }
        }

        public static string PipeName => "NamedPipe_" + typeof(T).Name;

        public event EventHandler<MessageEventArgs<T>>? MessageReceived;

        public void Dispose()
        {
            _pipeServer.Dispose();
            _logger.Debug("Finished listening");
        }

        private NamedPipeServerStream CreateListener()
        {
            _logger.Debug("Listening for the new message...");
            // Create the new async pipe
            var stream = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

            // Wait for a connection
            stream.BeginWaitForConnection(WaitForConnectionCallBack, _pipeServer);
            return stream;
        }

        private void WaitForConnectionCallBack(IAsyncResult iar)
        {
            try
            {
                // End waiting for the connection
                try
                {
                    _pipeServer.EndWaitForConnection(iar);
                }
                catch (ObjectDisposedException)
                {
                    //Normal termaination
                    return;
                }

                _logger.Debug("Receiving message from client...");
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
                _logger.Debug($"Message {message} is received");
                var handler = Volatile.Read(ref MessageReceived);
                if (handler == null)
                {
                    _logger.Debug("No handler is registered");
                }
                else
                {
                    _logger.Debug("Handling message");
                    handler.Invoke(this, new MessageEventArgs<T>(message));
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            _pipeServer = CreateListener();
        }
    }
}
