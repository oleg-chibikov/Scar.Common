using System;

namespace Scar.Common.NamedPipes
{
    public sealed class MessageEventArgs<T> : EventArgs
    {
        public MessageEventArgs(T message)
        {
            Message = message;
        }

        public T Message { get; }
    }
}
