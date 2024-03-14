using System;
using System.Reflection;
using Scar.Common.ApplicationLifetime.Contracts;

namespace Scar.Common.ApplicationLifetime.Core;

public class EntryAssemblyProvider : IEntryAssemblyProvider
{
    public Assembly ProvideEntryAssembly()
    {
        return Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Entry assembly is null");
    }
}