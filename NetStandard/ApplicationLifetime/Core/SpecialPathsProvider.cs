using System;
using Scar.Common.ApplicationLifetime.Contracts;

namespace Scar.Common.ApplicationLifetime.Core;

public class SpecialPathsProvider : ISpecialPathsProvider
{
    public string ProvideSpecialPath(Environment.SpecialFolder specialFolderType)
    {
        return Environment.GetFolderPath(specialFolderType);
    }
}