using io.fusionauth;


namespace blastcms.FusionAuthService
{
    public class FusionAuthFactory : IFusionAuthFactory
    {
        private readonly FusionAuthClient _client;
        public FusionAuthFactory(FusionAuthClient client)
        {
            _client = client;
        }

        public IFusionAuthAsyncClient GetClientWithTenant(string tenantId)
        {
            return _client.withTenantId(tenantId);
        }
    }
}
