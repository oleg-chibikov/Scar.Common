using System;
using System.IO;
using System.Security.Cryptography;

namespace Scar.Common.Cryptography;

public class FileHasher : IFileHasher
{
    public byte[] GetSha512Hash(string filePath)
    {
        _ = filePath ?? throw new ArgumentNullException(nameof(filePath));
        using var sha512 = SHA512.Create();
        using var stream = File.OpenRead(filePath);
        return sha512.ComputeHash(stream);
    }
}