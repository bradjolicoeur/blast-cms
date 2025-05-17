using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GetUrlRedirectByFrom
    {
        public class Query : IRequest<Model>
        {
            public Query(string redirectFrom)
            {
                RedirectFrom = redirectFrom;
            }

            public string RedirectFrom { get; }
        }

        public class Model
        {
            public Model(UrlRedirect data)
            {
                Data = data;
            }
            public UrlRedirect Data { get; }
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
                    var data = await session.Query<UrlRedirect>().FirstOrDefaultAsync(q => q.RedirectFrom.Equals(request.RedirectFrom, StringComparison.OrdinalIgnoreCase));

                    return new Model(data);
                }
            }

        }
    }
}
