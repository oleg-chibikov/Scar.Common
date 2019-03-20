using System;
using System.IO;
using System.Reflection;

namespace Scar.Common.IO
{
    public static class CommonPaths
    {
        public static string Company => ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyCompanyAttribute), false)).Company;
        public static string Product => ((AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyProductAttribute), false)).Product;
        public static string ProgramName => $"{Company}\\{Product}";
        public static string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProgramName);
    }
}