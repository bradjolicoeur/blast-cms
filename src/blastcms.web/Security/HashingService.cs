using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;

namespace blastcms.web.Security
{
    public class HashingService : IHashingService
    {
        public string HashValue(string base64string)
        {
            var salt = GenerateSalt(16);

            var bytes = KeyDerivation.Pbkdf2(base64string, salt, KeyDerivationPrf.HMACSHA512, 10000, 16);

            return $"{ Convert.ToBase64String(salt) }:{ Convert.ToBase64String(bytes) }";
        }

        public bool CheckMatch(string hash, string input)
        {
            try
            {
                var parts = hash.Split(':');

                var salt = Convert.FromBase64String(parts[0]);

                var bytes = KeyDerivation.Pbkdf2(input, salt, KeyDerivationPrf.HMACSHA512, 10000, 16);

                return parts[1].Equals(Convert.ToBase64String(bytes));
            }
            catch
            {
                return false;
            }
        }

        public Tuple<string, string> GenerateNewKey()
        {
            var value = Guid.NewGuid().ToString();
            var hashed = HashValue(value);

            return new Tuple<string, string>(hashed, value);
        }

        private static byte[] GenerateSalt(int length)
        {
            var salt = new byte[length];

            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }

            return salt;
        }
    }


}
