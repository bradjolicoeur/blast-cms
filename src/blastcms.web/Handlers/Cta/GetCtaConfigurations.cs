using blastcms.web.Data;
using Marten.Linq;
using Marten;
using blastcms.web.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System;

namespace blastcms.web.Handlers
{
    public class GetCtaConfigurations
    {
        public class Query : IRequest<PagedData>
        {
            public int Skip { get; }
            public int Take { get; }
            public int CurrentPage { get; }
            public string Search { get; }

            public Query(int skip, int take, int currentPage, string search = null)
            {
                Skip = skip;
                Take = take;
                CurrentPage = currentPage;
                Search = search;
            }
        }

        public class PagedData : IPagedData<CtaConfiguration>
        {
            public PagedData(IEnumerable<CtaConfiguration> data, long count, int page)
            {
                Data = data;
                Count = count;
                Page = page;
            }

            public IEnumerable<CtaConfiguration> Data { get; }
            public long Count { get; }
            public int Page { get; }
        }

        public class Handler : IRequestHandler<Query, PagedData>
        {
            private readonly ISessionFactory _sessionFactory;

            public Handler(ISessionFactory sessionFactory)
            {
                _sessionFactory = sessionFactory;
            }

            public async Task<PagedData> Handle(Query request, CancellationToken cancellationToken)
            {
                using var session = _sessionFactory.QuerySession();
                QueryStatistics stats = null;

                var search = request.Search ?? string.Empty;

                var items = await session.Query<CtaConfiguration>()
                    .Stats(out stats)
                    .Where(q => q.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                             || q.Slug.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .OrderBy(o => o.Name)
                    .ToListAsync(cancellationToken);

                return new PagedData(items, stats.TotalResults, request.CurrentPage);
            }
        }
    }
}
