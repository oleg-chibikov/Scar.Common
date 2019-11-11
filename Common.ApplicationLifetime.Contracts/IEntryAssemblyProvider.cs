using System.Reflection;

namespace Scar.Common.ApplicationLifetime.Contracts
{
    public interface IEntryAssemblyProvider
    {
        Assembly ProvideEntryAssembly();
    }
}