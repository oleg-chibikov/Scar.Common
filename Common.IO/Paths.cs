using System;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace Scar.Common.IO
{
    public static class CommonPaths
    {
        [NotNull]
        private const string DropboxInfoPath = @"Dropbox\info.json";

        [NotNull]
        private const string OneDriveNotDetected = @"ND";

        [NotNull]
        public static string Company => ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyCompanyAttribute), false)).Company;

        [NotNull]
        public static string Product => ((AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyProductAttribute), false)).Product;

        [NotNull]
        public static string ProgramName => $"{Company}\\{Product}";

        [NotNull]
        public static string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProgramName);

        [CanBeNull]
        public static string GetDropboxPath()
        {
            var jsonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DropboxInfoPath);
            if (!File.Exists(jsonPath))
            {
                jsonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DropboxInfoPath);
            }

            return !File.Exists(jsonPath) ? null : File.ReadAllText(jsonPath).Split('\"')[5].Replace(@"\\", @"\");
        }

        [CanBeNull]
        public static string GetOneDrivePath()
        {
            var path = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\OneDrive", "UserFolder", OneDriveNotDetected);
            return path == OneDriveNotDetected || string.IsNullOrWhiteSpace(path) ? null : path;
        }
    }
}