using blastcms.ArticleScanService.CaptureMeta;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace blastcms.ArticleScanService.Tests
{
    public class CaptureMetaContentFormatterTests
    {
        [Test]
        public void FormatHtml_includes_relevant_meta_and_rendered_body_content()
        {
            const string html = """
                <html>
                  <head>
                    <meta property="og:title" content="Medium Article Title" />
                    <meta property="og:description" content="Medium Description" />
                    <meta property="article:author" content="Author Name" />
                    <meta property="og:image" content="https://cdn.example.com/image.jpg" />
                  </head>
                  <body>
                    <article>
                      <h1>Rendered headline</h1>
                      <p>Rendered body content from Medium.</p>
                    </article>
                  </body>
                </html>
                """;

            var result = CaptureMetaContentFormatter.FormatHtml(html);

            StringAssert.Contains("og:title|Medium Article Title", result.Data);
            StringAssert.Contains("og:description|Medium Description", result.Data);
            StringAssert.Contains("article:author|Author Name", result.Data);
            StringAssert.Contains("og:image|https://cdn.example.com/image.jpg", result.Data);
            StringAssert.Contains("Rendered headline", result.Data);
            StringAssert.Contains("Rendered body content from Medium.", result.Data);
        }

        [Test]
        public void FormatHtml_returns_empty_for_cloudflare_verification_interstitial()
        {
            const string html = """
                <html>
                  <head>
                    <title>Just a moment...</title>
                    <meta property="og:title" content="Just a moment..." />
                    <meta property="og:description" content="This page verifies users to protect the site from malicious bots, displaying a message during the process." />
                  </head>
                  <body>
                    <div id="cf-challenge-running">
                      <h1>Checking your browser before accessing Medium.</h1>
                      <p>This page verifies users to protect the site from malicious bots, displaying a message during the process.</p>
                      <div>Cloudflare Ray ID: 1234567890</div>
                    </div>
                  </body>
                </html>
                """;

            var formatted = CaptureMetaContentFormatter.TryFormatHtml(html, out var tryFormattedResult);

            ClassicAssert.IsFalse(formatted);
            var result = CaptureMetaContentFormatter.FormatHtml(html);

            ClassicAssert.AreEqual(string.Empty, result.Data);
            ClassicAssert.AreEqual(string.Empty, tryFormattedResult.Data);
            ClassicAssert.IsTrue(CaptureMetaContentFormatter.LooksLikeVerificationInterstitial(html));
            StringAssert.DoesNotContain("malicious bots", result.Data);
            StringAssert.DoesNotContain("Cloudflare", result.Data);
        }

        [Test]
        public void FormatHtml_uses_main_element_when_no_article_present()
        {
            const string html = """
                <html>
                  <head>
                    <meta property="og:title" content="Page Title" />
                  </head>
                  <body>
                    <nav><a href="/">Home</a></nav>
                    <main>
                      <h1>Main Content Heading</h1>
                      <p>Main content here.</p>
                    </main>
                    <footer>Footer text</footer>
                  </body>
                </html>
                """;

            var result = CaptureMetaContentFormatter.FormatHtml(html);

            StringAssert.Contains("Main Content Heading", result.Data);
            StringAssert.Contains("Main content here", result.Data);
        }

        [Test]
        public void FormatHtml_prefers_article_over_main()
        {
            const string html = """
                <html>
                  <body>
                    <main>
                      <p>Main wrapper text</p>
                      <article>
                        <h1>Article Heading</h1>
                        <p>Article body text.</p>
                      </article>
                    </main>
                  </body>
                </html>
                """;

            var result = CaptureMetaContentFormatter.FormatHtml(html);

            StringAssert.Contains("Article Heading", result.Data);
        }

        [Test]
        public void FormatHtml_captures_name_attribute_meta_tags()
        {
            const string html = """
                <html>
                  <head>
                    <meta name="description" content="Page description via name" />
                    <meta name="twitter:title" content="Twitter Title" />
                    <meta name="twitter:image" content="https://example.com/img.jpg" />
                    <meta property="og:title" content="OG Title" />
                  </head>
                  <body><p>Content</p></body>
                </html>
                """;

            var result = CaptureMetaContentFormatter.FormatHtml(html);

            StringAssert.Contains("description|Page description via name", result.Data);
            StringAssert.Contains("twitter:title|Twitter Title", result.Data);
            StringAssert.Contains("twitter:image|https://example.com/img.jpg", result.Data);
            StringAssert.Contains("og:title|OG Title", result.Data);
        }

        [Test]
        public void FormatHtml_falls_back_to_full_body_when_no_article_or_main()
        {
            const string html = """
                <html>
                  <body>
                    <div>
                      <h1>Just a Div Heading</h1>
                      <p>Content in a plain div.</p>
                    </div>
                  </body>
                </html>
                """;

            var result = CaptureMetaContentFormatter.FormatHtml(html);

            StringAssert.Contains("Just a Div Heading", result.Data);
            StringAssert.Contains("Content in a plain div", result.Data);
        }

        [Test]
        public void LooksLikeVerificationInterstitialText_matches_cloudflare_challenge_copy()
        {
            const string text = "Checking your browser before accessing Medium. Cloudflare Ray ID: 1234567890.";

            ClassicAssert.IsTrue(CaptureMetaContentFormatter.LooksLikeVerificationInterstitialText(text));
        }
    }
}
