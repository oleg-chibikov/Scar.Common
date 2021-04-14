using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;

namespace Scar.Common.Cryptography
{
    /// <summary>
    /// The implementation is taken from https://stackoverflow.com/a/20622428.
    /// </summary>
    [SuppressMessage("Security", "CA5379:Do Not Use Weak Key Derivation Function Algorithm", Justification = "As is")]
    [SuppressMessage("Security", "CA5387:Use at least 10000 iterations", Justification = "As is")]
    public sealed class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            _ = password ?? throw new ArgumentNullException(nameof(password));
            byte[] salt;
            byte[] buffer2;
            using (var bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }

            var dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }

        public bool VerifyHashedPassword(string hashedPassword, string password)
        {
            _ = hashedPassword ?? throw new ArgumentNullException(nameof(hashedPassword));
            _ = password ?? throw new ArgumentNullException(nameof(password));
            byte[] buffer4;
            var src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0))
            {
                return false;
            }

            var dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            var buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
            using (var bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8))
            {
                buffer4 = bytes.GetBytes(0x20);
            }

            return buffer3.SequenceEqual(buffer4);
        }
    }
}
