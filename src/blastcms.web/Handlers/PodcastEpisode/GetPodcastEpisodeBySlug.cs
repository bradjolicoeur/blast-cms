using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GetPodcastEpisodeBySlug
    {
        public class Query : IRequest<Model>
        {
            public Query(string slug)
            {
                Slug = slug;
            }

            public string Slug { get; }
        }

        public class Model
        {
            public Model(PodcastEpisode episode)
            {
                Episode = episode;
            }
            public PodcastEpisode Episode { get; }
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
                    var data = await session.Query<PodcastEpisode>().FirstOrDefaultAsync(q => q.Slug.Equals(request.Slug, StringComparison.OrdinalIgnoreCase), token: cancellationToken);

                    return new Model(data);
                }
            }

        }
    }
}
