using blastcms.ArticleScanService;
using blastcms.ArticleScanService.CaptureMeta;
using blastcms.ArticleScanService.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Collections.Generic;

namespace blastcms.web.tests.Services
{
    public class ArticleScanServiceRegistrationTests
    {
        [Test]
        public void AddArticleScanService_registers_scraper_and_capture_services()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["OPENAI_KEY"] = "test-key"
                })
                .Build());

            services.AddArticleScanService();

            using var provider = services.BuildServiceProvider();

            var scraper = provider.GetRequiredService<IMetaScraper>();
            var factory = provider.GetRequiredService<ICaptureMetaFactory>();
            var htmlCapture = factory.GetCaptureMeta("https://example.com/post");
            var youtubeCapture = factory.GetCaptureMeta("https://www.youtube.com/watch?v=123");

            ClassicAssert.IsInstanceOf<MetaScraperOpenAI>(scraper);
            ClassicAssert.IsInstanceOf<CaptureHtmlPageMeta>(htmlCapture);
            ClassicAssert.IsInstanceOf<CaptureYouTubeMeta>(youtubeCapture);
        }
    }
}
