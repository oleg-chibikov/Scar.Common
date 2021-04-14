using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Scar.Common.ApplicationLifetime.Contracts;

namespace Scar.Common.ApplicationLifetime.Core
{
    public class AssemblyInfoProvider : IAssemblyInfoProvider
    {
        public AssemblyInfoProvider(IEntryAssemblyProvider entryAssemblyProvider, ISpecialPathsProvider specialPathsProvider, Guid? customGuid = null)
        {
            var entryAssembly = entryAssemblyProvider?.ProvideEntryAssembly() ?? throw new ArgumentNullException(nameof(entryAssemblyProvider));
            var localAppSettingsPath = specialPathsProvider?.ProvideSpecialPath(Environment.SpecialFolder.LocalApplicationData) ?? throw new ArgumentNullException(nameof(specialPathsProvider));

            Company = GetCustomAttribute<AssemblyCompanyAttribute>(entryAssembly).Company;
            Product = GetCustomAttribute<AssemblyProductAttribute>(entryAssembly).Product;
            ProgramName = Path.Combine(Company, Product);
            SettingsPath = Path.Combine(localAppSettingsPath, ProgramName);
            AppGuid = customGuid != null ? customGuid.ToString() ! : GetCustomAttribute<GuidAttribute>(entryAssembly).Value;
        }

        public string AppGuid { get; }

        public string Company { get; }

        public string Product { get; }

        public string ProgramName { get; }

        public string SettingsPath { get; }

        static T GetCustomAttribute<T>(Assembly entryAssembly)
            where T : Attribute
        {
            return (T)(Attribute.GetCustomAttribute(entryAssembly, typeof(T), false) ?? throw new InvalidOperationException());
        }
    }
}
