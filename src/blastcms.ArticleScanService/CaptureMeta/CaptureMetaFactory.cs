using Microsoft.Extensions.DependencyInjection;
using System;

namespace blastcms.ArticleScanService.CaptureMeta
{
    public class CaptureMetaFactory : ICaptureMetaFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CaptureMetaFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public ICaptureMeta GetCaptureMeta(string url)
        {
            var provider = GetProviderKey(url);

            return _serviceProvider.GetKeyedService<ICaptureMeta>(provider);
        }
        
        private string GetProviderKey(string url)
        {
            var uri = new Uri(url);
            var host = uri.Host;

            if (host.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) || host.Contains("youtu.be", StringComparison.OrdinalIgnoreCase))
                return "youtube";

            if (host.Contains("medium.com", StringComparison.OrdinalIgnoreCase))
                return "medium";

            return "htmlpage";
        }
    }
}
