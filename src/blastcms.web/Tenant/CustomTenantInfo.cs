using blastcms.web.Data;
using Finbuckle.MultiTenant;

namespace blastcms.web.Tenant
{
    public class CustomTenantInfo : TenantInfo
    {
        public string OpenIdConnectClientId { get; set; }
        public string OpenIdConnectAuthority { get; set; }
        public string OpenIdConnectClientSecret { get; set; }
        public string ChallengeScheme { get; set; }
        public string CustomerId { get; set; }
        public string ReferenceId { get; set; }
        public string IdentityTenantId { get; set; }
        public string IdentityApplicationId { get; set; }

        public bool AdminTenant { get; set; } = false;
        public BillingProvider BillingProvider {get; set;} = BillingProvider.None;
        public AuthenticationProvider AuthenticationProvider { get; set; } = AuthenticationProvider.FusionAuth;
        
    }


}
