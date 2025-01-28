using blastcms.web.Data;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
            var items = new List<BlastTenant>();

            if (configuration["ASPNETCORE_ENVIRONMENT"] == "Development")
            {
                items.AddRange(
                new[] {new BlastTenant{
                        Id = "unique-id-0ff4adaf",
                        Identifier = "customer2",
                        Name = "Tenant 1 Company Name",
                        ChallengeScheme = "OpenIdConnect",
                        OpenIdConnectClientId = configuration["Tenant1ClientId"],
                        OpenIdConnectAuthority = configuration["OIDCAuthority"],
                        OpenIdConnectClientSecret = configuration["Tenant1Secret"],
                        IdentityTenantId = "d7d09513-a3f5-401c-9685-34ab6c552453",
                },
                new BlastTenant{
                        Id = "unique-id-ao41n44",
                        Identifier = "tenant-2",
                        Name = "Name of Tenant 2",
                        ChallengeScheme = "OpenIdConnect",
                        OpenIdConnectClientId = configuration["Tenant2ClientId"],
                        OpenIdConnectAuthority = configuration["OIDCAuthority"],
                        OpenIdConnectClientSecret = configuration["Tenant2Secret"],
                        IdentityTenantId = "d7d09513-a3f5-401c-9685-34ab6c552453",
                }
                });
            }

            items.Add(new BlastTenant
            {
                Id = "unique-id-admin",
                Identifier = configuration["TenantAdminIdentifier"] ?? "admin",
                Name = "Administrative Tenant",
                ChallengeScheme = "OpenIdConnect",
                OpenIdConnectClientId = configuration["TenantAdminClientId"],
                OpenIdConnectAuthority = configuration["OIDCAuthority"],
                OpenIdConnectClientSecret = configuration["TenantAdminSecret"],
                IdentityTenantId = configuration["IdentityTenantId"] ?? "d7d09513-a3f5-401c-9685-34ab6c552453",
                AdminTenant = true, 
            });

            return items.ToArray();
        }

      

    }
}
