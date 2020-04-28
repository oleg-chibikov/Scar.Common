using System.Reflection;
using Scar.Common.ApplicationLifetime.Contracts;

namespace Scar.Common.ApplicationLifetime.Core
{
    public class EntryAssemblyProvider : IEntryAssemblyProvider
    {
        public Assembly ProvideEntryAssembly() => Assembly.GetEntryAssembly();
    }
}
