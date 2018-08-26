using System;
using JetBrains.Annotations;
using Scar.Common.Exceptions;

namespace Scar.Common.Messages
{
    public sealed class Message
    {
        public Message([NotNull] string text, MessageType type, [CanBeNull] Exception exception = null)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Type = type;
            Exception = exception;
        }

        public Message([NotNull] LocalizableException exception)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            Text = exception.LocalizedMessage;
            Type = MessageType.Warning;
        }

        public Message([NotNull] Exception exception)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            Text = exception.Message;
            Type = MessageType.Error;
        }

        [CanBeNull]
        public Exception Exception { get; }

        public string Text { get; }

        public MessageType Type { get; }

        public override string ToString()
        {
            return Text;
        }
    }
}