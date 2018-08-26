using System;
using JetBrains.Annotations;

namespace Scar.Common.Exceptions
{
    /// <inheritdoc />
    /// <summary>
    /// Exception, which was caused by the abscence of some resource
    /// </summary>
    public class NotFoundException : LocalizableException
    {
        public NotFoundException([NotNull] string localizedMessage, [CanBeNull] string message = null)
            : base(localizedMessage, message)
        {
        }

        public NotFoundException([NotNull] string localizedMessage, [NotNull] Exception innerException, [CanBeNull] string message = null)
            : base(localizedMessage, innerException, message)
        {
        }
    }
}