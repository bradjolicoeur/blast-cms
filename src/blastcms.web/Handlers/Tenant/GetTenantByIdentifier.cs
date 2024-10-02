using AutoMapper;
using blastcms.web.Data;
using Marten;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace blastcms.web.Handlers
{
    public class GetTenantByIdentifier
    {
        public class Query : IRequest<Model>
        {
            public Query(string identifier)
            {
                Identifier = identifier;
            }

            public string Identifier { get; }
        }

        public class Model
        {
            public Model(BlastTenant tenant)
            {
                Tenant = tenant;
            }
            public BlastTenant Tenant { get; }
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
                
                var tenant = await session.Query<BlastTenant>()
                    .FirstOrDefaultAsync(q => q.Identifier.Equals(request.Identifier, StringComparison.OrdinalIgnoreCase), cancellationToken);

                return new Model(tenant);
           
            }

        }
    }
}
