using blastcms.web.Data;
using blastcms.web.Helpers;
using Marten;
using Marten.Linq;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GetSitemapItems
    {
        public class Query : IRequest<PagedData>
        {
            public int Skip { get; internal set; }
            public int Take { get; internal set; }
            public int CurrentPage { get; internal set; }
            public string Search { get; internal set; }

            public Query(int skip, int take, int currentPage, string search = null)
            {
                Skip = skip;
                Take = take;
                CurrentPage = currentPage;
                Search = search;
            }
        }

        public class PagedData : IPagedData<SitemapItem>
        {
            public PagedData(IEnumerable<SitemapItem> data, long count, int page)
            {
                Data = data;
                Count = count;
                Page = page;
            }

            public IEnumerable<SitemapItem> Data { get; }
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
                {
                    QueryStatistics stats = null;

                    var query = session.Query<SitemapItem>()
                        .Stats(out stats)

                        .If(!string.IsNullOrWhiteSpace(request.Search), 
                            x => x.Where(q => q.RelativePath.Contains(request.Search, StringComparison.OrdinalIgnoreCase)))

                        .Skip(request.Skip)
                        .Take(request.Take)
                        .OrderByDescending(o => o.LastModified).AsQueryable();

                    var articles = await query.ToListAsync();

                    return new PagedData(articles, stats.TotalResults, request.CurrentPage);
                }
            }

        }
    }
}
