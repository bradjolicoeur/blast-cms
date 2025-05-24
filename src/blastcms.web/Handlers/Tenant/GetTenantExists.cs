using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace blastcms.web.Handlers.Tenant
{
    public class GetTenantExists
    {
        public class Query : IRequest<Model>
        {
            public Query(string identifier)
            {
                Identifier = identifier;
            }

            public string Identifier { get; }
        }

        public record Model(bool Exists)
        {
        }


        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly IDocumentStore _documentStore;

            public Handler(IDocumentStore documentStore)
            {
                _documentStore = documentStore;
            }

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                using var session = _documentStore.QuerySession();
                {
                    var tenant = await session.Query<BlastTenant>()
                   .FirstOrDefaultAsync(q => q.Identifier.Equals(request.Identifier, StringComparison.OrdinalIgnoreCase), cancellationToken);

                    return new Model(tenant != null);
                }
            }

        }
    }
}
