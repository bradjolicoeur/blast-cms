using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace blastcms.ArticleScanService.CaptureMeta
{
    public class CaptureHtmlPageMeta(ILogger<CaptureHtmlPageMeta> logger ) : ICaptureMeta
    {
        private static readonly string[] LinuxUserAgents = [
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36",
            "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:123.0) Gecko/20100101 Firefox/123.0",
            "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:122.0) Gecko/20100101 Firefox/122.0",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            "Mozilla/5.0 (X11; Fedora; Linux x86_64; rv:123.0) Gecko/20100101 Firefox/123.0",
            "Mozilla/5.0 (X11; Linux x86_64; rv:109.0) Gecko/20100101 Firefox/121.0",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36"
        ];

        private static int _userAgentIndex = 0;
        private static readonly object _lock = new();

        private static string GetNextUserAgent()
        {
            lock (_lock)
            {
                var userAgent = LinuxUserAgents[_userAgentIndex];
                _userAgentIndex = (_userAgentIndex + 1) % LinuxUserAgents.Length;
                return userAgent;
            }
        }

        public async Task<CaptureMetaResult> GetMeta(string url)
        {
            var uri = new Uri(url);
            logger.LogInformation("Scraping: {url}", url);
            // Use HttpClient with a browser-like User-Agent
            using var httpClient = new HttpClient();
            ConfigureBrowserHeaders(httpClient);
            
            using var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var document = new HtmlDocument();
            // Load directly from stream to preserve encoding from Content-Type header and meta tags
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                document.Load(stream, true);
            }
            
            logger.LogInformation("Captured content from: {url}", url);
            return CaptureMetaContentFormatter.FormatDocument(document);
        }

        private static void ConfigureBrowserHeaders(HttpClient httpClient)
        {
            var userAgent = GetNextUserAgent();
            httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
            httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "identity");
            httpClient.DefaultRequestHeaders.Add("DNT", "1");
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");

            if (userAgent.Contains("Chrome"))
            {
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
            }

            httpClient.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
        }
    }
}
