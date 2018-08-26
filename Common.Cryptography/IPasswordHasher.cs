using JetBrains.Annotations;

namespace Scar.Common.Cryptography
{
    public interface IPasswordHasher
    {
        [NotNull]
        string HashPassword([NotNull] string password);

        bool VerifyHashedPassword([NotNull] string hashedPassword, [NotNull] string providedPassword);
    }
}