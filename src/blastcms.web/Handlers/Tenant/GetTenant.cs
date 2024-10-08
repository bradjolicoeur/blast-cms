using AutoMapper;
using blastcms.web.Data;
using Marten;
using MediatR;
using System.Threading.Tasks;
using System.Threading;

namespace blastcms.web.Handlers
{
    public class GetTenant
    {
        public class Query : IRequest<Model>
        {
            public Query(string id)
            {
                Id = id;
            }

            public string Id { get; }
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
                {
                    var article = await session.LoadAsync<BlastTenant>(request.Id, cancellationToken);

                    return new Model(article);
                }
            }

        }
    }
}
