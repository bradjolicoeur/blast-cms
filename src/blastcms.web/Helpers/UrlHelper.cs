

using Microsoft.Extensions.Configuration;

namespace blastcms.web.Helpers
{
    public static class UrlHelper
    {
        public const string IMAGE_BASE_URL = "https://storage.googleapis.com";
        
        public static string Bucket { get; set; }

        public static string GetFullImageUrl(this string imagePath)
        {
            return $"{IMAGE_BASE_URL}/{Bucket}/{imagePath}";
        }
    }
}
