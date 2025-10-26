using System;
using System.Security.Cryptography;
using System.Text;

namespace C_C_Final.Resources.Utils
{
    internal static class HashFunction
    {
        public static string ComputeHash(string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(value);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
