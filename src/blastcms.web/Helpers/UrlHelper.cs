

using Microsoft.Extensions.Configuration;

namespace blastcms.web.Helpers
{
    public static class UrlHelper
    {
        public const string IMAGE_BASE_URL = "https://storage.googleapis.com";
        
        public static IConfiguration Configuration { get; set; }

        public static string GetFullImageUrl(this string imagePath)
        {
            var bucket = Configuration["GoogleCloudStorageBucket"];
            return $"{IMAGE_BASE_URL}/{bucket}/{imagePath}";
        }
    }
}
