using blastcms.ArticleScanService.CaptureMeta;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace blastcms.web.tests.Services
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
    }
}
