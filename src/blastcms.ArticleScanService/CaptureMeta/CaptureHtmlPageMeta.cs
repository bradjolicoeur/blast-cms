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
        public async Task<CaptureMetaResult> GetMeta(string url)
        {
            var uri = new Uri(url);
            logger.LogInformation("Scraping: {url}", url);
            // Use HttpClient with a browser-like User-Agent
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
            var html = await httpClient.GetStringAsync(url);
            var document = new HtmlDocument();
            document.LoadHtml(html);
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
