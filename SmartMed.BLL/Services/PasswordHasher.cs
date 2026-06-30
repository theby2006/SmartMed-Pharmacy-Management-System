using System;
using System.Security.Cryptography;
using SmartMed.BLL.Interfaces;
using SmartMed.Common.Configuration;
using SmartMed.Common.Constants;
using SmartMed.Common.Helpers;

namespace SmartMed.BLL.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly int _iterations;

        public PasswordHasher()
        {
            _iterations = AppSettings.GetRequiredInt(ConfigKeys.HashIterations);
        }

        public string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            return Convert.ToBase64String(saltBytes);
        }

        public string HashPassword(string password, string salt)
        {
            Guard.AgainstNullOrWhiteSpace(password, nameof(password));
            Guard.AgainstNullOrWhiteSpace(salt, nameof(salt));

            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] hashBytes;

            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, _iterations, HashAlgorithmName.SHA256))
            {
                hashBytes = pbkdf2.GetBytes(32);
            }

            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string password, string hash, string salt)
        {
            Guard.AgainstNullOrWhiteSpace(password, nameof(password));
            Guard.AgainstNullOrWhiteSpace(hash, nameof(hash));
            Guard.AgainstNullOrWhiteSpace(salt, nameof(salt));

            string computedHash = HashPassword(password, salt);

            return ConstantTimeComparison(Convert.FromBase64String(computedHash), Convert.FromBase64String(hash));
        }

        private static bool ConstantTimeComparison(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }
    }
}
