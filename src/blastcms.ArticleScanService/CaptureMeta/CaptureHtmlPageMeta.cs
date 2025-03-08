using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using ReverseMarkdown;
using System;
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
            // Get the html from URL specified
            var webGet = new HtmlWeb();
            var document = await webGet.LoadFromWebAsync(url);
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
