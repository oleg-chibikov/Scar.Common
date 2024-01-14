using System;
using Scar.Common.Exceptions;

namespace Scar.Common.Messages;

public static class MessageExtensions
{
    public static Message ToError(this string text, Exception? exception = null)
    {
        return new Message(text, MessageType.Error, exception);
    }

    public static Message ToMessage(this string text)
    {
        return new Message(text, MessageType.Message);
    }

    public static Message ToMessage(this LocalizableException localizableException)
    {
        _ = localizableException ?? throw new ArgumentNullException(nameof(localizableException));
        return new Message(localizableException);
    }

    public static Message ToMessage(this Exception exception)
    {
        _ = exception ?? throw new ArgumentNullException(nameof(exception));
        return exception is LocalizableException localizableException ? new Message(localizableException) : new Message(exception);
    }

    public static Message ToWarning(this string text, Exception? exception = null)
    {
        return new Message(text, MessageType.Warning, exception);
    }

    public static Message ToSuccess(this string text)
    {
        return new Message(text, MessageType.Success);
    }
}