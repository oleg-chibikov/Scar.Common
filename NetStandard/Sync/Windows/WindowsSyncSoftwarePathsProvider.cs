using System;
using System.IO;
using Microsoft.Win32;
using Scar.Common.Sync.Contracts;

namespace Scar.Common.Sync.Windows
{
    public class WindowsSyncSoftwarePathsProvider : IOneDrivePathProvider, IDropBoxPathProvider
    {
        const string DropboxInfoPath = @"Dropbox\info.json";
        const string OneDriveNotDetected = @"ND";

        public string? GetDropBoxPath()
        {
            var jsonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DropboxInfoPath);
            if (!File.Exists(jsonPath))
            {
                jsonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DropboxInfoPath);
            }

            return !File.Exists(jsonPath) ? null : File.ReadAllText(jsonPath).Split('\"')[5].Replace(@"\\", @"\", StringComparison.Ordinal);
        }

        public string? GetOneDrivePath()
        {
            var path = (string?)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\OneDrive", "UserFolder", OneDriveNotDetected);
            return (path == OneDriveNotDetected) || string.IsNullOrWhiteSpace(path) ? null : path;
        }
    }
}
