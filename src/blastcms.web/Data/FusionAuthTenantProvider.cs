using blastcms.FusionAuthService;
using blastcms.web.Tenant;

namespace blastcms.web.Data
{
    public class FusionAuthTenantProvider : IFusionAuthTenantProvider
    {
        private readonly CustomTenantInfo _tenantInfo;

        public FusionAuthTenantProvider(CustomTenantInfo tenantInfo) 
        {
            _tenantInfo = tenantInfo;
        }
        public string GetTenantId()
        {
            return _tenantInfo.IdentityTenantId;
        }
    }
}
