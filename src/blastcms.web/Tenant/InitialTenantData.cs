using blastcms.web.Data;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Tenant
{
    public class InitialData : IInitialData
    {
        private readonly object[] _initialData;

        public InitialData(params object[] initialData)
        {
            _initialData = initialData;
        }

        public async Task Populate(IDocumentStore store, CancellationToken cancellation)
        {
            await using var session = store.LightweightSession();
            // Marten UPSERT will cater for existing records
            session.Store(_initialData);
            await session.SaveChangesAsync();
        }
    }

    internal static class InitialDatasets
    {
        public static  BlastTenant[] Tenants(IConfiguration configuration) 
        {

            return new BlastTenant[] { 
                new BlastTenant{
                        Id = "unique-id-0ff4adaf",
                        Identifier = "customer2",
                        Name = "Tenant 1 Company Name",
                        ChallengeScheme = "OpenIdConnect",
                        OpenIdConnectClientId = "4bca19bf-e584-4732-b7d5-b5a23fec3f96",
                        OpenIdConnectAuthority = "https://fusion.blastcms.net",
                        OpenIdConnectClientSecret = configuration["Tenant1Secret"] },
                new BlastTenant{
                        Id = "unique-id-ao41n44",
                        Identifier = "tenant-2",
                        Name = "Name of Tenant 2",
                        ChallengeScheme = "OpenIdConnect",
                        OpenIdConnectClientId = "114a8f3d-56cb-4a0f-b710-8eadf30635a2",
                        OpenIdConnectAuthority = "https://fusion.blastcms.net",
                        OpenIdConnectClientSecret = configuration["Tenant2Secret"] },
            };
        }

    }
}
