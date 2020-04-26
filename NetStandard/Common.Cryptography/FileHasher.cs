using System;
using System.IO;
using System.Security.Cryptography;

namespace Scar.Common.Cryptography
{
    public class FileHasher : IFileHasher
    {
        // The cryptographic service provider.
        readonly SHA512 _sha512 = SHA512.Create();

        // Compute the file's hash.
        public byte[] GetSha512Hash(string filePath)
        {
            _ = filePath ?? throw new ArgumentNullException(nameof(filePath));
            using var stream = File.OpenRead(filePath);
            return _sha512.ComputeHash(stream);
        }
    }
}
