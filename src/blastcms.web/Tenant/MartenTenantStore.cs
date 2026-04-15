using blastcms.web.Data;
using blastcms.web.Handlers;
using Finbuckle.MultiTenant.Abstractions;
using blastcms.web.Infrastructure;
using Microsoft.Extensions.Configuration;
using Riok.Mapperly.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blastcms.web.Tenant
{
    public partial class MartenTenantStore : IMultiTenantStore<CustomTenantInfo>
    {
        [Mapper]
        public partial class SliceMapper
        {
            [MapperIgnoreTarget(nameof(CustomTenantInfo.IdentityApplicationId))]
            [MapperIgnoreTarget(nameof(CustomTenantInfo.BillingProvider))]
            [MapperIgnoreTarget(nameof(CustomTenantInfo.AuthenticationProvider))]
            public partial CustomTenantInfo ToTenantInfo(BlastTenant source);
        }

        private static readonly SliceMapper Mapper = new();
        private readonly IDispatcher _mediator;
        private readonly HostTenantConfig _hostTenantConfig;

        public MartenTenantStore(IDispatcher mediator, HostTenantConfig hostTenantConfig)
        {
            _mediator = mediator;
            _hostTenantConfig = hostTenantConfig;
        }

        public async Task<IEnumerable<CustomTenantInfo>> GetAllAsync()
        {
            var response = await _mediator.Send(new GetTenants.Query(0,100,1));

            if (response == null) return null;

            var tenants = response.Data.Select(Mapper.ToTenantInfo).ToArray();
            foreach(var item in tenants)
            {
                item.BillingProvider = _hostTenantConfig.BillingProvider;
                item.AuthenticationProvider = _hostTenantConfig.AuthenticationProvider;
            }
            return tenants;
        }

        public Task<bool> TryAddAsync(CustomTenantInfo tenantInfo)
        {
            throw new System.NotImplementedException();
        }

        public async Task<CustomTenantInfo> TryGetAsync(string id)
        {
            var response = await _mediator.Send(new GetTenant.Query(id));

            if (response == null) return null;

            var tenant = Mapper.ToTenantInfo(response.Tenant);
            tenant.AuthenticationProvider = _hostTenantConfig.AuthenticationProvider;
            tenant.BillingProvider = _hostTenantConfig.BillingProvider;
            return tenant;
        }

        public async Task<CustomTenantInfo> TryGetByIdentifierAsync(string identifier)
        {
            var response = await _mediator.Send(new GetTenantByIdentifier.Query(identifier));

            if (response == null) return null;

            var tenant = Mapper.ToTenantInfo(response.Tenant);
            tenant.AuthenticationProvider = _hostTenantConfig.AuthenticationProvider;
            tenant.BillingProvider = _hostTenantConfig.BillingProvider;
            return tenant;
        }

        public Task<bool> TryRemoveAsync(string identifier)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> TryUpdateAsync(CustomTenantInfo tenantInfo)
        {
            throw new System.NotImplementedException();
        }
    }
}
