using blastcms.UserManagement;
using io.fusionauth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


namespace blastcms.FusionAuthService.ExtensionHelpers
{
    public static class FusionAuthServiceExtension
    {
        public static IServiceCollection AddFusionAuth(this IServiceCollection services, Action<FusionAuthOptions> setupAction)
        {
            services.Configure(setupAction);
            services.AddScoped(provider =>
            {
                var options = provider.GetRequiredService<IOptions<FusionAuthOptions>>();
                return new FusionAuthClient(options.Value.FusionAuthApiKey, options.Value.FusionAuthApiUrl);
            });

            services.AddTransient<IFusionAuthFactory, FusionAuthFactory>();
            services.AddTransient<IUserManagementProvider, FusionAuthUserManagementProvider>();

            return services;
        }
    }
}
