using AutoMapper;
using blastcms.web.Data;
using blastcms.web.Infrastructure;
using Marten;
using Marten.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GetContentTags
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

        public class PagedData : IPagedData<ContentTag>
        {
            public PagedData(IEnumerable<ContentTag> data, long count, int page)
            {
                Data = data;
                Count = count;
                Page = page;
            }

            public IEnumerable<ContentTag> Data { get; }
            public long Count { get; }
            public int Page { get; }
        }


      

        public class Handler : IRequestHandler<Query, PagedData>
        {
            private readonly ISessionFactory _sessionFactory;
            private readonly IMapper _mapper;

            public Handler(ISessionFactory sessionFactory, IMapper mapper)
            {
                _sessionFactory = sessionFactory;
                _mapper = mapper;
            }

            public async Task<PagedData> Handle(Query request, CancellationToken cancellationToken)
            {
                using var session = _sessionFactory.QuerySession();
                {
                    QueryStatistics stats = null;

                    var articles = await session.Query<ContentTag>()
                        .Stats(out stats)
                        .Where(q => q.Value.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                        .Skip(request.Skip)
                        .Take(request.Take)
                        .OrderBy(o => o.Value)
                        .ToListAsync();

                    return new PagedData(articles, stats.TotalResults, request.CurrentPage);
                }
            }

        }
    }
}
