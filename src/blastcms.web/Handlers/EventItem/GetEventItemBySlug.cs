using blastcms.web.Data;
using Marten;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GetEventItemBySlug
    {
        public class Query : IRequest<Model>
        {
            public Query(string slug)
            {
                Slug = slug;
            }

            public string Slug { get; }
        }

        public record Model(EventItem Data)
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
                    var data = await session.Query<EventItem>().FirstAsync(q => q.Slug.Equals(request.Slug, StringComparison.OrdinalIgnoreCase), token: cancellationToken);

                    return new Model(data);
                }
            }

        }
    }
}
