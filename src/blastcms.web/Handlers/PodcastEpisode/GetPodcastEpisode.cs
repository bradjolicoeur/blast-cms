using blastcms.web.Data;
using Marten;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GetPodcastEpisode
    {
        public class Query : IRequest<Model>
        {
            public Query(Guid id)
            {
                Id = id;
            }

            public Guid Id { get; }
        }

        public record Model(PodcastEpisode Episode)
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
                    var item = await session.LoadAsync<PodcastEpisode>(request.Id, cancellationToken);

                    return new Model(item);
                }
            }

        }
    }
}
