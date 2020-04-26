using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Scar.Common
{
    public static class NugetHelper
    {
        static readonly Regex NupkgRegex = new Regex("^(.*?)\\.((?:\\.?[0-9]+){3,}(?:[-a-z]+)?)\\.nupkg$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static NugetPackageInfo? ParseNugetPackageInfoForPath(string filePath)
        {
            _ = filePath ?? throw new ArgumentNullException(nameof(filePath));

            var fileName = Path.GetFileName(filePath);
            return ParseNugetPackageInfo(fileName);
        }

        public static NugetPackageInfo? ParseNugetPackageInfo(string fileName)
        {
            _ = fileName ?? throw new ArgumentNullException(nameof(fileName));
            var matches = NupkgRegex.Matches(fileName);
            if (matches.Count != 1)
            {
                return null;
            }

            var match = matches[0];

            if (match.Groups.Count != 3)
            {
                return null;
            }

            var name = match.Groups[1].Value;
            if (!Version.TryParse(match.Groups[2].Value, out var version))
            {
                return null;
            }

            return new NugetPackageInfo(name, version);
        }
    }
}
