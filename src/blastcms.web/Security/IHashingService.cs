using System;

namespace blastcms.web.Security
{
    public interface IHashingService
    {
        bool CheckMatch(string hash, string input);
        string HashValue(string base64string);
        Tuple<string, string> GenerateNewKey();
    }
}