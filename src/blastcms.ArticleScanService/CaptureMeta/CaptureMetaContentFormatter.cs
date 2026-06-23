using HtmlAgilityPack;
using ReverseMarkdown;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace blastcms.ArticleScanService.CaptureMeta
{
    public static class CaptureMetaContentFormatter
    {
        private static readonly string[] VerificationInterstitialPhrases =
        [
            "this page verifies users to protect the site from malicious bots",
            "verify you are human",
            "verify you are a human",
            "checking your browser",
            "checking if the site connection is secure",
            "please wait while we verify",
            "enable javascript and cookies to continue",
            "just a moment",
            "cloudflare"
        ];

        public static CaptureMetaResult FormatHtml(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return FormatDocument(document);
        }

        public static bool TryFormatHtml(string html, out CaptureMetaResult result)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return TryFormatDocument(document, out result);
        }

        public static CaptureMetaResult FormatDocument(HtmlDocument document)
        {
            if (LooksLikeVerificationInterstitial(document))
            {
                return new CaptureMetaResult(string.Empty);
            }

            var body = document.DocumentNode.SelectSingleNode("//body");
            var metaTags = document.DocumentNode.SelectNodes("//meta");
            var metaText = ConvertMetaTagsToText(metaTags);
            var markdownText = ConvertToMarkdown(body);

            return new CaptureMetaResult(MergeMetaAndMarkdown(metaText, markdownText));
        }

        public static bool TryFormatDocument(HtmlDocument document, out CaptureMetaResult result)
        {
            if (LooksLikeVerificationInterstitial(document))
            {
                result = new CaptureMetaResult(string.Empty);
                return false;
            }

            result = FormatDocument(document);
            return true;
        }

        public static bool LooksLikeVerificationInterstitial(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return LooksLikeVerificationInterstitial(document);
        }

        public static bool LooksLikeVerificationInterstitial(HtmlDocument document)
        {
            if (document?.DocumentNode == null)
            {
                return false;
            }

            var combinedText = Normalize(document.DocumentNode.InnerText);
            var title = Normalize(document.DocumentNode.SelectSingleNode("//title")?.InnerText);
            var metaText = Normalize(ConvertMetaTagsToText(document.DocumentNode.SelectNodes("//meta")));

            var hasVerificationLanguage = VerificationInterstitialPhrases.Any(phrase =>
                combinedText.Contains(phrase, StringComparison.Ordinal)
                || title.Contains(phrase, StringComparison.Ordinal)
                || metaText.Contains(phrase, StringComparison.Ordinal));

            var hasCloudflareMarker = combinedText.Contains("cloudflare", StringComparison.Ordinal)
                                      || title.Contains("just a moment", StringComparison.Ordinal)
                                      || document.DocumentNode.SelectSingleNode("//*[contains(@id,'challenge') or contains(@class,'cf-')]") != null
                                      || document.DocumentNode.SelectSingleNode("//form[contains(@action,'/cdn-cgi/challenge-platform')]") != null;

            return hasVerificationLanguage && hasCloudflareMarker;
        }

        public static bool LooksLikeVerificationInterstitialText(string text)
        {
            var normalized = Normalize(text);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            var hasVerificationLanguage = VerificationInterstitialPhrases.Any(normalized.Contains);
            var hasCloudflareMarker = normalized.Contains("cloudflare", StringComparison.Ordinal)
                                      || normalized.Contains("just a moment", StringComparison.Ordinal)
                                      || normalized.Contains("cf-challenge", StringComparison.Ordinal)
                                      || normalized.Contains("challenge-platform", StringComparison.Ordinal)
                                      || normalized.Contains("cdn-cgi/challenge-platform", StringComparison.Ordinal);

            return hasVerificationLanguage && hasCloudflareMarker;
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

        private static string Normalize(string text)
        {
            var value = Regex.Replace(text ?? string.Empty, "\\s+", " ");
            return value.Trim().ToLowerInvariant();
        }
    }
}
