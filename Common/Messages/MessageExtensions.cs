using System;
using JetBrains.Annotations;
using Scar.Common.Exceptions;

namespace Scar.Common.Messages
{
    public static class MessageExtensions
    {
        [NotNull]
        public static Message ToError([NotNull] this string text, [CanBeNull] Exception exception = null)
        {
            return new Message(text, MessageType.Error, exception);
        }

        [NotNull]
        public static Message ToMessage([NotNull] this string text)
        {
            return new Message(text, MessageType.Message);
        }

        [NotNull]
        public static Message ToMessage([NotNull] this LocalizableException localizableException)
        {
            if (localizableException == null)
            {
                throw new ArgumentNullException(nameof(localizableException));
            }

            return new Message(localizableException);
        }

        [NotNull]
        public static Message ToMessage([NotNull] this Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            return exception is LocalizableException localizableException ? new Message(localizableException) : new Message(exception);
        }

        [NotNull]
        public static Message ToWarning([NotNull] this string text, [CanBeNull] Exception exception = null)
        {
            return new Message(text, MessageType.Warning, exception);
        }

        [NotNull]
        public static Message ToSuccess([NotNull] this string text)
        {
            return new Message(text, MessageType.Success);
        }
    }
}