using AutoMapper;
using blastcms.web.Data;
using Marten;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Threading;

namespace blastcms.web.Handlers
{
    public class AlterTenant
    {
        public class Command : IRequest<Model>
        {
            [Required]
            public string Id { get; set; }

            [Required]
            public string Identifier { get; set; }

            [Required]
            public string Name { get; set; }

            [Required]
            public string OpenIdConnectClientId { get; set; }

            [Required]
            public string OpenIdConnectAuthority { get; set; }

            [Required]
            public string OpenIdConnectClientSecret { get; set; }

            [Required]
            public string ChallengeScheme { get; set; }

        }

        public class Model
        {
            public Model(BlastTenant tenant)
            {
                Tenant = tenant;
            }

            public BlastTenant Tenant { get; }
        }


        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Command, BlastTenant>().ReverseMap();
            }
        }

        public class Handler : IRequestHandler<Command, Model>
        {

            private readonly IMapper _mapper;

            private readonly IDocumentStore _documentStore;

            public Handler(IDocumentStore documentStore, IMapper mapper)
            {
                _documentStore = documentStore;
                _mapper = mapper;
            }

            public async Task<Model> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenant = _mapper.Map<BlastTenant>(request);

                using var session = _documentStore.OpenSession();
                {
                    session.Store(tenant);

                    await session.SaveChangesAsync();

                    return new Model(tenant);
                }
            }

        }
    }
}
