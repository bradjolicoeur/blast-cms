using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;
using System.Text;

namespace blastcms.web.Security
{
    public class HashingService : IHashingService
    {
        public string HashValue(string base64string)
        {
            var salt = GenerateSalt(16);

            return HashValue(base64string, salt);
            
        }

        public string HashValue(string base64string, byte[] salt)
        {
            var bytes = KeyDerivation.Pbkdf2(base64string, salt, KeyDerivationPrf.HMACSHA512, 10000, 16);

            return $"{ Convert.ToBase64String(salt) }:{ Convert.ToBase64String(bytes) }";
        }

        public string RegenHash(string input)
        {
            var encodedTextBytes = Convert.FromBase64String(input);
            string plainText = Encoding.UTF8.GetString(encodedTextBytes);
            var parts = plainText.Split(':');
            var salt = Convert.FromBase64String(parts[0]);

            return HashValue(parts[1], salt);
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
            var salt = GenerateSalt(16);
            var hashed = HashValue(value,salt);

            var plainTextBytes = Encoding.UTF8.GetBytes($"{ Convert.ToBase64String(salt) }:{ value }");
            var withsalt = Convert.ToBase64String(plainTextBytes);

            return new Tuple<string, string>(hashed, withsalt);
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
