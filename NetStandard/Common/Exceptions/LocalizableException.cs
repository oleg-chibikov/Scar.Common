using System;

namespace Scar.Common.Exceptions;

/// <inheritdoc />
/// <summary>
/// Exception, which was caused by user input and thus should have a meaningful message.
/// </summary>
public class LocalizableException : Exception
{
    public LocalizableException()
    {
        throw new NotSupportedException();
    }

    public LocalizableException(string message) : base(message)
    {
        throw new NotSupportedException();
    }

    public LocalizableException(string message, Exception innerException) : base(message, innerException)
    {
        throw new NotSupportedException();
    }

    public LocalizableException(string localizedMessage, string? message = null) : base(message)
    {
        LocalizedMessage = localizedMessage ?? throw new ArgumentNullException(nameof(localizedMessage));
    }

    public LocalizableException(string localizedMessage, Exception innerException, string? message = null) : base(message, innerException)
    {
        LocalizedMessage = localizedMessage ?? throw new ArgumentNullException(nameof(localizedMessage));
    }

    public string LocalizedMessage { get; }
}