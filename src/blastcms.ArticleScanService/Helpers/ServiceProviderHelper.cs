using blastcms.ArticleScanService.CaptureMeta;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace blastcms.ArticleScanService.Helpers
{
    public static class ServiceProviderHelper
    {
        public static IServiceCollection AddArticleScanService(this IServiceCollection services)
        {
            services.AddKeyedSingleton<ICaptureMeta, CaptureYouTubeMeta>("youtube");
            services.AddKeyedSingleton<ICaptureMeta, CaptureHtmlPageMeta>("htmlpage");
            services.AddSingleton<ICaptureMetaFactory, CaptureMetaFactory>();
            services.AddTransient<IMetaScraper, MetaScraperOpenAI>();
            // Register a named HttpClient for CaptureHtmlPageMeta
            services.AddHttpClient<CaptureHtmlPageMeta>(client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
            });
            services.AddHttpClient();

            return services;
        }
    }
}
