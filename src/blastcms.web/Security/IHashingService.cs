using System;

namespace blastcms.web.Security
{
    public interface IHashingService
    {
        bool CheckMatch(string hash, string input);
        string HashValue(string base64string);
        string HashValue(string base64string, byte[] salt);
        Tuple<string, string> GenerateNewKey();
        string RegenHash(string input);
    }
}