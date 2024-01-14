using System;
using Scar.Common.Exceptions;

namespace Scar.Common.Messages;

public sealed class Message
{
    public Message(string text, MessageType type, Exception? exception = null)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
        Type = type;
        Exception = exception;
    }

    public Message(LocalizableException exception)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        Text = exception.LocalizedMessage;
        Type = MessageType.Warning;
    }

    public Message(Exception exception)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        Text = exception.Message;
        Type = MessageType.Error;
    }

    public Exception? Exception { get; }

    public string Text { get; }

    public MessageType Type { get; }

    public override string ToString()
    {
        return Text;
    }
}