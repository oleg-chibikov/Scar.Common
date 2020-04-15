using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Scar.Common.IO
{
    public static class PathExtensions
    {
        public static string SanitizePath(this string path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            return new Regex($"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()))}]", RegexOptions.Compiled).Replace(path, string.Empty);
        }
    }
}