using System;
using Scar.Common.Exceptions;

namespace Scar.Common.Messages;

public sealed class Message(string text, MessageType type, Exception? exception = null)
{
#pragma warning disable CA1062 // Validate arguments of public methods
    public Message(LocalizableException exception) : this(
        exception.LocalizedMessage,
        MessageType.Warning,
        exception ?? throw new ArgumentNullException(nameof(exception)))
    {
    }

    public Message(Exception exception) : this(
        exception.Message,
        MessageType.Error,
        exception ?? throw new ArgumentNullException(nameof(exception)))
    {
    }
#pragma warning restore CA1062 // Validate arguments of public methods

    public Exception? Exception { get; } = exception;

    public string Text { get; } = text ?? throw new ArgumentNullException(nameof(text));

    public MessageType Type { get; } = type;

    public override string ToString()
    {
        return Text;
    }
}
