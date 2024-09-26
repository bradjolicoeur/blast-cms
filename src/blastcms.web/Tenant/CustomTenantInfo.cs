using Finbuckle.MultiTenant;

namespace blastcms.web.Tenant
{
    public class CustomTenantInfo : TenantInfo
    {
        public string OpenIdConnectClientId { get; set; }
        public string OpenIdConnectAuthority { get; set; }
        public string OpenIdConnectClientSecret { get; set; }
        public string ChallengeScheme { get; set; }
    }
}
