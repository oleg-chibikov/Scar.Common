using System;

namespace Scar.Common.ApplicationLifetime.Contracts
{
    public interface ISpecialPathsProvider
    {
        string ProvideSpecialPath(Environment.SpecialFolder specialFolderType);
    }
}