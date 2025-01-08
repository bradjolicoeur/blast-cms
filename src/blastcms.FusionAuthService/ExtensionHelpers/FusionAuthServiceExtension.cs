using io.fusionauth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blastcms.FusionAuthService.ExtensionHelpers
{
    public static class FusionAuthServiceExtension
    {
        public static IServiceCollection AddFusionAuth(this IServiceCollection services, Action<FusionAuthOptions> setupAction)
        {
            services.Configure(setupAction);
            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<FusionAuthOptions>>();
                return new FusionAuthClient(options.Value.FusionAuthApiKey, options.Value.FusionAuthApiUrl);
            });

            return services;
        }
    }
}
