using blastcms.ArticleScanService.CaptureMeta;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace blastcms.ArticleScanService.Helpers
{
    public static class ServiceProviderHelper
    {
        public static IServiceCollection AddArticleScanService(this IServiceCollection services)
        {
            services.AddKeyedSingleton<ICaptureMeta, CaptureYouTubeMeta>("youtube");
            services.AddKeyedSingleton<ICaptureMeta, CaptureMediumArticleMeta>("medium");
            services.AddKeyedSingleton<ICaptureMeta, CaptureHtmlPageMeta>("htmlpage");
            services.AddSingleton<ICaptureMetaFactory, CaptureMetaFactory>();
            services.AddTransient<IMetaScraper, MetaScraperOpenAI>();
            services.AddHttpClient(CaptureHtmlPageMeta.BrowserHttpClientName, client =>
            {
                client.Timeout = TimeSpan.FromSeconds(20);
            });
            services.AddHttpClient();

            return services;
        }
    }
}
