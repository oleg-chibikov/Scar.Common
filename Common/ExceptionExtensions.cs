using System;
using JetBrains.Annotations;

namespace Scar.Common
{
    public static class ExceptionExtensions
    {
        [NotNull]
        public static Exception GetMostInnerException([NotNull] this Exception e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            while (e.InnerException != null)
            {
                e = e.InnerException;
            }

            return e;
        }
    }
}