using System;

namespace Scar.Common
{
    public static class StringExtensions
    {
        public static string? CapitalizeIfNotEmpty(this string? str)
        {
            return (str == null) || string.IsNullOrWhiteSpace(str) ? str : Capitalize(str);
        }

        public static string Capitalize(this string str)
        {
            _ = str ?? throw new ArgumentNullException(nameof(str));

            str = str.Trim();
            if (str.Length > 1)
            {
                return char.ToUpperInvariant(str[0]) + str[1..];
            }

            return str.ToUpperInvariant();
        }
    }
}
