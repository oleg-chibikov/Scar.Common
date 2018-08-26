using JetBrains.Annotations;

namespace Scar.Common
{
    public static class StringExtensions
    {
        [CanBeNull]
        public static string Capitalize([CanBeNull] this string str)
        {
            str = str?.Trim();
            if (str != null && str.Length > 1)
            {
                return char.ToUpper(str[0]) + str.Substring(1);
            }

            return str?.ToUpper();
        }
    }
}