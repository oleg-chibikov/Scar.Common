using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Scar.Common.ApplicationLifetime.Contracts;

namespace Scar.Common.ApplicationLifetime
{
    public class AssemblyInfoProvider : IAssemblyInfoProvider
    {
        public AssemblyInfoProvider(IEntryAssemblyProvider entryAssemblyProvider, ISpecialPathsProvider specialPathsProvider, Guid? customGuid = null)
        {
            var entryAssembly = entryAssemblyProvider?.ProvideEntryAssembly() ?? throw new ArgumentNullException(nameof(entryAssemblyProvider));
            var localAppSettingsPath = specialPathsProvider?.ProvideSpecialPath(Environment.SpecialFolder.LocalApplicationData)
                                       ?? throw new ArgumentNullException(nameof(specialPathsProvider));

            Company = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyCompanyAttribute), false)).Company;
            Product = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyProductAttribute), false)).Product;
            ProgramName = Path.Combine(Company, Product);
            SettingsPath = Path.Combine(localAppSettingsPath, ProgramName);
            AppGuid = customGuid != null ? customGuid.ToString() : ((GuidAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(GuidAttribute), false)).Value;
        }

        public string AppGuid { get; }
        public string Company { get; }
        public string Product { get; }
        public string ProgramName { get; }
        public string SettingsPath { get; }
    }
}