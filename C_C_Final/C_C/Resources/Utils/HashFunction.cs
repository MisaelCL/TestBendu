using System;
using System.Security.Cryptography;

namespace C_C_Final.Resources.Utils
{
    /// <summary>
    /// Gestiona el hashing y la verificación de contraseñas usando PBKDF2.
    /// </summary>
    public class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int HashSize = 32; // 256 bit
        private const int Iterations = 10000;

        /// <summary>
        /// Crea un hash (y su salt) a partir de una contraseña.
        /// </summary>
        /// <param name="password">La contraseña a hashear.</param>
        /// <returns>Un tupla con el Hash (Base64) y el Salt (Base64).</returns>
        public (string Hash, string Salt) HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            using (var rng = RandomNumberGenerator.Create())
            {
                var saltBytes = new byte[SaltSize];
                rng.GetBytes(saltBytes);

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
                {
                    var hashBytes = pbkdf2.GetBytes(HashSize);
                    return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
                }
            }
        }

        /// <summary>
        /// Verifica si una contraseña coincide con un hash y salt almacenados.
        /// </summary>
        /// <param name="password">La contraseña a verificar.</param>
        /// <param name="storedHashBase64">El hash almacenado (en Base64).</param>
        /// <param name="storedSaltBase64">El salt almacenado (en Base64).</param>
        /// <returns>True si la contraseña es válida, false en caso contrario.</returns>
        public bool VerifyPassword(string password, string storedHashBase64, string storedSaltBase64)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHashBase64) || string.IsNullOrEmpty(storedSaltBase64))
            {
                return false;
            }

            try
            {
                var saltBytes = Convert.FromBase64String(storedSaltBase64);
                var expectedHashBytes = Convert.FromBase64String(storedHashBase64);

                if (saltBytes.Length != SaltSize || expectedHashBytes.Length != HashSize)
                {
                    return false;
                }

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
                {
                    var actualHashBytes = pbkdf2.GetBytes(HashSize);
                    return CryptographicOperations.FixedTimeEquals(actualHashBytes, expectedHashBytes);
                }
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
