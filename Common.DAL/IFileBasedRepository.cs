using JetBrains.Annotations;

namespace Scar.Common.DAL
{
    public interface IFileBasedRepository
    {
        [NotNull]
        string DbDirectoryPath { get; }

        [NotNull]
        string DbFileExtension { get; }

        [NotNull]
        string DbFileName { get; }
    }
}