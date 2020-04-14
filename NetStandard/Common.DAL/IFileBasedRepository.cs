namespace Scar.Common.DAL
{
    public interface IFileBasedRepository
    {
        string DbDirectoryPath { get; }

        string DbFileExtension { get; }

        string DbFileName { get; }
    }
}
