using System;

namespace Scar.Common
{
    public static class ExceptionExtensions
    {
        public static Exception GetMostInnerException(this Exception e)
        {
            _ = e ?? throw new ArgumentNullException(nameof(e));
            while (e.InnerException != null)
            {
                e = e.InnerException;
            }

            return e;
        }
    }
}