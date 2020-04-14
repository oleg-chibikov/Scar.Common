using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Scar.Common.IO
{
    public static class DirectoryExtensions
    {
        static readonly char[] DirectorySeparators =
        {
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        };

        public static string AddTrailingBackslash(this string directoryPath)
        {
            _ = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));

            // They're always one character but EndsWith is shorter than
            // array style access to last directoryPath character. Change this
            // if performance are a (measured) issue.
            var separator1 = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
            var separator2 = Path.AltDirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);

            // Trailing white spaces are always ignored but folders may have
            // leading spaces. It's unusual but it may happen. If it's an issue
            // then just replace TrimEnd() with Trim(). Tnx Paul Groke to point this out.
            directoryPath = directoryPath.TrimEnd();

            // Argument is always a directory name then if there is one
            // of allowed separators then I have nothing to do.
            if (directoryPath.EndsWith(separator1, StringComparison.OrdinalIgnoreCase) || directoryPath.EndsWith(separator2, StringComparison.OrdinalIgnoreCase))
            {
                return directoryPath;
            }

            // If there is the "alt" separator then I add a trailing one.
            // Note that URI format (file://drive:\directoryPath\filename.ext) is
            // not supported in most .NET I/O functions then we don't support it
            // here too. If you have to then simply revert this check:
            // if (directoryPath.Contains(separator1))
            // return directoryPath + separator1;
            // return directoryPath + separator2;
            if (directoryPath.Contains(separator2))
            {
                return directoryPath + separator2;
            }

            // If there is not an "alt" separator I add a "normal" one.
            // It means directoryPath may be with normal one or it has not any separator
            // (for example if it's just a directory name). In this case I
            // default to normal as users expect.
            return directoryPath + separator1;
        }

        public static IEnumerable<string> GetFiles(this string directoryPath, string searchPatternExpression = "", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            _ = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
            _ = searchPatternExpression ?? throw new ArgumentNullException(nameof(searchPatternExpression));
            var searchPatternRegex = new Regex(searchPatternExpression, RegexOptions.IgnoreCase);
            return Directory.EnumerateFiles(directoryPath, "*", searchOption)
                .Where(
                    filePath =>
                    {
                        var extension = Path.GetExtension(filePath);
                        return (extension != null) && searchPatternRegex.IsMatch(extension);
                    });
        }

        // Takes same patterns, and executes in parallel
        public static IEnumerable<string> GetFiles(this string directoryPath, string[] searchPatterns, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            _ = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
            _ = searchPatterns ?? throw new ArgumentNullException(nameof(searchPatterns));
            return searchPatterns.AsParallel().SelectMany(searchPattern => Directory.EnumerateFiles(directoryPath, searchPattern, searchOption));
        }

        public static void OpenDirectoryInExplorer(this string directoryPath)
        {
            _ = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            Process.Start(directoryPath);
        }

        public static string RemoveTrailingBackslash(this string directoryPath)
        {
            _ = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
            return directoryPath.TrimEnd(DirectorySeparators);
        }
    }
}
