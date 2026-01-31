using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using ReverseMarkdown;
using System;
using System.Net.Http;
using System.Text;
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
            
            var body = document.DocumentNode.SelectSingleNode("//body");
            var metaTags = document.DocumentNode.SelectNodes("//meta");

            logger.LogInformation("Captured content from: {url}", url);
            //convert the meta tags to plain text so that it can be added to prompt
            var metaText = ConvertMetaTagsToText(metaTags);

            //Convert the body to markdown text so that we are sending plain text content in prompt
            string markdownText = ConvertToMarkdown(body);

            //Merge the meta and markdown together for the prompt
            string textToSummarize = MergeMetaAndMarkdown(metaText, markdownText);

            return new CaptureMetaResult(textToSummarize);
        }

        private static void ConfigureBrowserHeaders(HttpClient httpClient)
        {
            var userAgent = GetNextUserAgent();
            httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
            httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            // Remove or limit Accept-Encoding to avoid compression issues with HTML parsing
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "identity");
            httpClient.DefaultRequestHeaders.Add("DNT", "1");
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            
            // Only add Chromium-specific headers for Chrome user agents
            if (userAgent.Contains("Chrome"))
            {
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
            }
            
            httpClient.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
        }

        private static string MergeMetaAndMarkdown(string metaText, string markdownText)
        {
            //merge the content and meta text together
            var sbAllText = new StringBuilder();
            sbAllText.AppendLine(metaText);
            sbAllText.AppendLine(markdownText);
            var textToSummarize = sbAllText.ToString();
            return textToSummarize;
        }

        private static string ConvertMetaTagsToText(HtmlNodeCollection metaTags)
        {
            string metaText = string.Empty;

            if (metaTags != null)
            {
                var sb = new StringBuilder();
                foreach (var item in metaTags)
                {
                    var prop = item.GetAttributeValue("property", "").ToLower();
                    if (prop.Contains("description") || prop.Contains("title")
                    || prop.Contains("author") || prop.Contains("keywords") || prop.Contains("image"))
                    {
                        sb.Append(prop);
                        sb.Append('|');
                        sb.Append(item.GetAttributeValue("content", ""));
                        sb.AppendLine();
                    }
                }
                metaText = sb.ToString();
            }

            return metaText;
        }

        private static string ConvertToMarkdown(HtmlNode body)
        {
            if (body == null)
                return string.Empty;

            var config = new ReverseMarkdown.Config
            {
                UnknownTags = Config.UnknownTagsOption.Drop
            };

            var converter = new ReverseMarkdown.Converter(config);
            string html = body.OuterHtml;

            string markdownText = converter.Convert(html);
            return markdownText;
        }
    }
}
