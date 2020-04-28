namespace Scar.Common.DAL.Contracts
{
    public interface IFileBasedRepository
    {
        string DbDirectoryPath { get; }

        string DbFileExtension { get; }

        string DbFileName { get; }
    }
}
