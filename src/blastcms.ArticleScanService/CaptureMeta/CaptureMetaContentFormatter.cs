using HtmlAgilityPack;
using ReverseMarkdown;
using System.Text;

namespace blastcms.ArticleScanService.CaptureMeta
{
    public static class CaptureMetaContentFormatter
    {
        public static CaptureMetaResult FormatHtml(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return FormatDocument(document);
        }

        public static CaptureMetaResult FormatDocument(HtmlDocument document)
        {
            var body = document.DocumentNode.SelectSingleNode("//body");
            var metaTags = document.DocumentNode.SelectNodes("//meta");
            var metaText = ConvertMetaTagsToText(metaTags);
            var markdownText = ConvertToMarkdown(body);

            return new CaptureMetaResult(MergeMetaAndMarkdown(metaText, markdownText));
        }

        private static string MergeMetaAndMarkdown(string metaText, string markdownText)
        {
            var sbAllText = new StringBuilder();
            sbAllText.AppendLine(metaText);
            sbAllText.AppendLine(markdownText);
            return sbAllText.ToString();
        }

        private static string ConvertMetaTagsToText(HtmlNodeCollection metaTags)
        {
            if (metaTags == null)
            {
                return string.Empty;
            }

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

            return sb.ToString();
        }

        private static string ConvertToMarkdown(HtmlNode body)
        {
            if (body == null)
            {
                return string.Empty;
            }

            var config = new Config
            {
                UnknownTags = Config.UnknownTagsOption.Drop
            };

            var converter = new Converter(config);
            var article = body.SelectSingleNode(".//article");
            var htmlToConvert = article?.InnerHtml ?? body.InnerHtml;

            return converter.Convert(htmlToConvert);
        }
    }
}
