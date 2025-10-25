using BCrypt.Net;

namespace C_C.Resources.utils;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
        {
            throw new ArgumentException("Password cannot be empty", nameof(plainText));
        }

        return BCrypt.Net.BCrypt.HashPassword(plainText, workFactor: WorkFactor);
    }

    public bool Verify(string plainText, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(plainText, hash);
    }
}
