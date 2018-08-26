using System;
using JetBrains.Annotations;

namespace Scar.Common.Exceptions
{
    /// <inheritdoc />
    /// <summary>
    /// Exception, which was caused by user input and thus should have a meaningful message
    /// </summary>
    public class LocalizableException : Exception
    {
        public LocalizableException([NotNull] string localizedMessage, [CanBeNull] string message = null)
            : base(message)
        {
            LocalizedMessage = localizedMessage ?? throw new ArgumentNullException(nameof(localizedMessage));
        }

        public LocalizableException([NotNull] string localizedMessage, [NotNull] Exception innerException, [CanBeNull] string message = null)
            : base(message, innerException)
        {
            LocalizedMessage = localizedMessage ?? throw new ArgumentNullException(nameof(localizedMessage));
        }

        [NotNull]
        public string LocalizedMessage { get; }
    }
}