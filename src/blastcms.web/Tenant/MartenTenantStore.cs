using AutoMapper;
using blastcms.web.Data;
using blastcms.web.Handlers;
using Finbuckle.MultiTenant.Abstractions;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace blastcms.web.Tenant
{
    public class MartenTenantStore : IMultiTenantStore<CustomTenantInfo>
    {
        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<BlastTenant, CustomTenantInfo>().ReverseMap();
            }
        }

        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly HostTenantConfig _hostTenantConfig;

        public MartenTenantStore(IMediator mediator, IMapper mapper, HostTenantConfig hostTenantConfig)
        {
            _mediator = mediator;
            _mapper = mapper;
            _hostTenantConfig = hostTenantConfig;
        }

        public async Task<IEnumerable<CustomTenantInfo>> GetAllAsync()
        {
            var response = await _mediator.Send(new GetTenants.Query(0,100,1));

            if (response == null) return null;

            var tenants = _mapper.Map<IEnumerable<CustomTenantInfo>>(response.Data);
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

            var tenant =  _mapper.Map<CustomTenantInfo>(response.Tenant);
            tenant.AuthenticationProvider = _hostTenantConfig.AuthenticationProvider;
            tenant.BillingProvider = _hostTenantConfig.BillingProvider;
            return tenant;
        }

        public async Task<CustomTenantInfo> TryGetByIdentifierAsync(string identifier)
        {
            var response = await _mediator.Send(new GetTenantByIdentifier.Query(identifier));

            if (response == null) return null;

            var tenant = _mapper.Map<CustomTenantInfo>(response.Tenant);
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
