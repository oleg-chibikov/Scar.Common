using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Scar.Common.Cryptography
{
    public static class HashingUtilities
    {
        public static byte[] GetHash(this string inputString)
        {
            using HashAlgorithm algorithm = SHA512.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(this string inputString)
        {
            return GetHashString(GetHash(inputString));
        }

        public static string GetHashString(this byte[] bytes)
        {
            _ = bytes ?? throw new ArgumentNullException(nameof(bytes));
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("X2", CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }
    }
}
