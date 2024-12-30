using blastcms.web.Data;
using Microsoft.Extensions.Configuration;

namespace blastcms.web.Tenant
{
    public class HostTenantConfig
    {
        public BillingProvider BillingProvider { get; }
        public AuthenticationProvider AuthenticationProvider { get; }   

        public HostTenantConfig(BillingProvider billingProvider, AuthenticationProvider authenticationProvider)
        {
            BillingProvider = billingProvider;
            AuthenticationProvider = authenticationProvider;
        }
        public HostTenantConfig(IConfiguration configuration)
        {
            BillingProvider = BillingProvider.FromName(configuration["BillingProviderName"] ?? BillingProvider.None.Name) ;
            AuthenticationProvider = AuthenticationProvider.FromName(configuration["AuthenticationProviderName"] ?? AuthenticationProvider.None.Name) ;
        }
    }
}
