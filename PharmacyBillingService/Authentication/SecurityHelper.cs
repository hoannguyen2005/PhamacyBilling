using System;
using System.Security.Cryptography;
using System.Text;

namespace PharmacyBillingService.Authentication
{
    public static class SecurityHelper
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32;  // 256 bit
        private const int Iterations = 10000;

        /// <summary>
        /// Hashes a password using PBKDF2 with a random salt.
        /// </summary>
        public static string HashPassword(string password)
        {
            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                SaltSize,
                Iterations,
                HashAlgorithmName.SHA256))
            {
                var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
                var salt = Convert.ToBase64String(algorithm.Salt);

                // Format: Iterations.Salt.Key
                return $"{Iterations}.{salt}.{key}";
            }
        }

        /// <summary>
        /// Verifies a password against a hashed password.
        /// </summary>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var parts = hashedPassword.Split('.', 3);
            if (parts.Length != 3)
            {
                return false;
            }

            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = parts[2];

            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256))
            {
                var keyToCheck = Convert.ToBase64String(algorithm.GetBytes(KeySize));
                return keyToCheck == key;
            }
        }
    }
}
