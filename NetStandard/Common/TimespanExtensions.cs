using System;

namespace Scar.Common
{
    public static class TimespanExtensions
    {
        public static string ToReadableFormat(this TimeSpan t)
        {
            if (t.TotalSeconds < 1.0)
            {
                return string.Format("{0:s\\.ff} s", t);
            }

            if (t.TotalMinutes < 1.0)
            {
                return string.Format("{0} s", t.TotalSeconds);
            }

            if (t.TotalHours < 1.0)
            {
                return string.Format("{0} m", t.TotalMinutes);
            }

            if (t.TotalDays < 1.0)
            {
                return string.Format("{0} h", t.TotalHours);
            }

            return string.Format("{0:%d} d", t);
        }
    }
}