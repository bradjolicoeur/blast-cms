using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using System.Threading.Tasks;
using System.Threading;

namespace blastcms.web.Handlers
{
    public class GetCtaConfigurationBySlug
    {
        public class Query : IRequest<Model>
        {
            public Query(string slug)
            {
                Slug = slug;
            }

            public string Slug { get; }
        }

        public record Model(CtaConfiguration Data);

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
                var data = await session.Query<CtaConfiguration>()
                    .FirstOrDefaultAsync(q => q.Slug == request.Slug, token: cancellationToken);

                return new Model(data);
            }
        }
    }
}
