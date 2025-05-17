using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GetPodcast
    {
        public class Query : IRequest<Model>
        {
            public Query(Guid id)
            {
                Id = id;
            }

            public Guid Id { get; }
        }

        public record Model(Podcast Podcast)
        {
           
        }


        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly ISessionFactory _sessionFactory;

            public Handler(ISessionFactory sessionFactory)
            {
                _sessionFactory = sessionFactory;
            }

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                using var session = _sessionFactory.QuerySession();
                {
                    var item = await session.LoadAsync<Podcast>(request.Id, cancellationToken);

                    return new Model(item);
                }
            }

        }
    }
}
