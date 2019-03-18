using System;

namespace Scar.Common.Exceptions
{
    /// <inheritdoc />
    /// <summary>
    /// Exception, which was caused by the absence of some resource
    /// </summary>
    public class NotFoundException : LocalizableException
    {
        public NotFoundException(string localizedMessage, string? message = null)
            : base(localizedMessage, message)
        {
        }

        public NotFoundException(string localizedMessage, Exception innerException, string? message = null)
            : base(localizedMessage, innerException, message)
        {
        }
    }
}