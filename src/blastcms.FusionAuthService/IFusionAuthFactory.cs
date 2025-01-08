using io.fusionauth;

namespace blastcms.FusionAuthService
{
    public interface IFusionAuthFactory
    {
        IFusionAuthAsyncClient GetClientWithTenant(string tenantId);
    }
}