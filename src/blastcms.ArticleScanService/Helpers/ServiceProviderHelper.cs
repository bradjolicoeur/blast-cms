using blastcms.ArticleScanService.CaptureMeta;
using Microsoft.Extensions.DependencyInjection;

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

            return services;
        }
    }
}
