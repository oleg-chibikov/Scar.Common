using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Scar.Common.IO;

public static class PathExtensions
{
    public static string SanitizePath(this string path)
    {
        _ = path ?? throw new ArgumentNullException(nameof(path));
        return new Regex($"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()))}]", RegexOptions.Compiled).Replace(path, string.Empty);
    }

    public static void OpenPathWithDefaultAction(this string path)
    {
        _ = path ?? throw new ArgumentNullException(nameof(path));
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo(path) { UseShellExecute = true };
        process.Start();
    }

    public static string CreateTempDirectoryInDriveRoot(this string directoryPath)
    {
        var tempDirectoryPath = GetTempDirectoryPathInDriveRoot(directoryPath);
        while (Directory.Exists(tempDirectoryPath))
        {
            tempDirectoryPath = GetTempDirectoryPathInDriveRoot(directoryPath);
        }

        Directory.CreateDirectory(tempDirectoryPath);
        return tempDirectoryPath;
    }

    public static string GetTempDirectoryPathInDriveRoot(string path)
    {
        return Path.Combine(Path.GetPathRoot(path) ?? throw new InvalidOperationException(), Path.GetRandomFileName());
    }

    public static string CreateTempDirectory()
    {
        var tempDirectoryPath = GetTempDirectoryPath();
        while (Directory.Exists(tempDirectoryPath))
        {
            tempDirectoryPath = GetTempDirectoryPath();
        }

        Directory.CreateDirectory(tempDirectoryPath);
        return tempDirectoryPath;
    }

    public static string GetTempDirectoryPath()
    {
        return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    }
}