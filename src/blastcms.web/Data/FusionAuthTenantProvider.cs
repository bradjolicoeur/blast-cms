using blastcms.FusionAuthService;
using blastcms.web.Tenant;
using Finbuckle.MultiTenant.Abstractions;

namespace blastcms.web.Data
{
    public class FusionAuthTenantProvider : IFusionAuthTenantProvider
    {
        private readonly IMultiTenantContextAccessor<CustomTenantInfo> _httpContextAccessor;

        public FusionAuthTenantProvider(IMultiTenantContextAccessor<CustomTenantInfo> httpContextAccessor) 
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string GetTenantId()
        {
            return _httpContextAccessor.MultiTenantContext?.TenantInfo?.Id;
        }
    }
}
