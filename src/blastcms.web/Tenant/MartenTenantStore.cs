using AutoMapper;
using blastcms.web.Data;
using blastcms.web.Handlers;
using Finbuckle.MultiTenant.Abstractions;
using MediatR;
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

        public MartenTenantStore(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CustomTenantInfo>> GetAllAsync()
        {
            var response = await _mediator.Send(new GetTenants.Query(0,100,1));

            if (response == null) return null;

            return _mapper.Map<IEnumerable<CustomTenantInfo>>(response.Data);
        }

        public Task<bool> TryAddAsync(CustomTenantInfo tenantInfo)
        {
            throw new System.NotImplementedException();
        }

        public async Task<CustomTenantInfo> TryGetAsync(string id)
        {
            var response = await _mediator.Send(new GetTenant.Query(id));

            if (response == null) return null;

            return _mapper.Map<CustomTenantInfo>(response.Tenant);
        }

        public async Task<CustomTenantInfo> TryGetByIdentifierAsync(string identifier)
        {
            var response = await _mediator.Send(new GetTenantByIdentifier.Query(identifier));

            if (response == null) return null;

            return _mapper.Map<CustomTenantInfo>(response.Tenant);
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
