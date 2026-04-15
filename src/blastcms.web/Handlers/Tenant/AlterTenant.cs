using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using Riok.Mapperly.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Threading;

namespace blastcms.web.Handlers
{
    public partial class AlterTenant
    {
        public class Command : IRequest<Model>
        {
            [Required]
            public string Id { get; set; }

            [Required]
            public string Identifier { get; set; }

            [Required]
            public string Name { get; set; }
            public string CustomerId { get; set; }
            public string ReferenceId { get; set; }
            public string IdentityTenantId { get; set; }

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


        [Mapper]
        public partial class SliceMapper
        {
            [MapperIgnoreTarget(nameof(BlastTenant.AdminTenant))]
            public partial BlastTenant ToTenant(Command source);

            [MapperIgnoreSource(nameof(BlastTenant.AdminTenant))]
            public partial Command ToCommand(BlastTenant source);
        }

        public class Handler : IRequestHandler<Command, Model>
        {
            private static readonly SliceMapper Mapper = new();

            private readonly IDocumentStore _documentStore;

            public Handler(IDocumentStore documentStore)
            {
                _documentStore = documentStore;
            }

            public async Task<Model> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenant = Mapper.ToTenant(request);

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
