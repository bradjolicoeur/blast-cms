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
            if (url.Contains("www.youtube.com")|| url.Contains("youtu.be"))
                return "youtube";

            return "htmlpage";
        }
    }
}
