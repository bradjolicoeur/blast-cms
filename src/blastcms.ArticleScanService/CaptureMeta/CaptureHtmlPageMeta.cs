using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace blastcms.ArticleScanService.CaptureMeta
{
    public class CaptureHtmlPageMeta(ILogger<CaptureHtmlPageMeta> logger, IHttpClientFactory httpClientFactory) : ICaptureMeta
    {
        public const string BrowserHttpClientName = "capture-meta-browser";

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
            _ = new Uri(url);
            logger.LogInformation("Scraping: {url}", url);
            var document = await LoadDocumentAsync(httpClientFactory, url);

            logger.LogInformation("Captured content from: {url}", url);
            return CaptureMetaContentFormatter.FormatDocument(document);
        }

        internal static async Task<HtmlDocument> LoadDocumentAsync(IHttpClientFactory httpClientFactory, string url)
        {
            using var httpClient = CreateBrowserClient(httpClientFactory);
            using var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var document = new HtmlDocument();
            using var stream = await response.Content.ReadAsStreamAsync();
            document.Load(stream, true);

            return document;
        }

        internal static HttpClient CreateBrowserClient(IHttpClientFactory httpClientFactory)
        {
            var httpClient = httpClientFactory.CreateClient(BrowserHttpClientName);
            ConfigureBrowserHeaders(httpClient);
            return httpClient;
        }

        internal static void ConfigureBrowserHeaders(HttpClient httpClient)
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
