using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Win32;
using Scar.Common.Sync.Contracts;

namespace Scar.Common.Sync.Windows
{
    public class WindowsSyncSoftwarePathsProvider : IOneDrivePathProvider, IDropBoxPathProvider
    {
        const string OneDriveNotDetected = @"ND";
        static readonly string DropboxInfoPath = Path.Combine("Dropbox", "info.json");

        public string? GetDropBoxPath()
        {
            var jsonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DropboxInfoPath);
            if (!File.Exists(jsonPath))
            {
                jsonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DropboxInfoPath);
            }

            return !File.Exists(jsonPath) ? null : File.ReadAllText(jsonPath).Split('\"')[5].Replace(@"\\", @"\", StringComparison.Ordinal);
        }

        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Windows-only")]
        public string? GetOneDrivePath()
        {
            var path = (string?)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\OneDrive", "UserFolder", OneDriveNotDetected);
            return (path == OneDriveNotDetected) || string.IsNullOrWhiteSpace(path) ? null : path;
        }
    }
}
