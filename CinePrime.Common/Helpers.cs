using System;
using System.Security.Cryptography;
using System.Text;

namespace CinePrime.Common
{
    public static class Helpers
    {
        public static string HashPassword(string plainText)
        {
            if (plainText == null)
            {
                return string.Empty;
            }

            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(plainText);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public static bool VerifyPassword(string plainText, string hash)
        {
            var computed = HashPassword(plainText);
            return string.Equals(computed, hash, StringComparison.Ordinal);
        }
    }
}
