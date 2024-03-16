namespace Scar.Common.Cryptography;

public interface IFileHasher
{
    byte[] GetSha512Hash(string filePath);
}
