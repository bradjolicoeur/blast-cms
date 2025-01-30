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
            if(tenantId == null) 
                throw new ArgumentNullException(nameof(tenantId));

            //Calling this with a null tenant id will return the connection with the defualt tenant
            return _client.withTenantId(tenantId);
        }
    }
}
