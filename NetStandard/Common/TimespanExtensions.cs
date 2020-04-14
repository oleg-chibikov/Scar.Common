using System;

namespace Scar.Common
{
    public static class TimespanExtensions
    {
        public static string ToReadableFormat(this TimeSpan t)
        {
            if (t.TotalSeconds < 1.0)
            {
                return $"{t:s\\.ff} s";
            }

            if (t.TotalMinutes < 1.0)
            {
                return $"{t.TotalSeconds} s";
            }

            if (t.TotalHours < 1.0)
            {
                return $"{t.TotalMinutes} m";
            }

            if (t.TotalDays < 1.0)
            {
                return $"{t.TotalHours} h";
            }

            return $"{t:%d} d";
        }
    }
}
