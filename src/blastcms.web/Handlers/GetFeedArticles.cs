using AutoMapper;
using blastcms.web.Data;
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
    public class GetFeedArticles
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

        public class PagedData : IPagedData<FeedArticle>
        {
            public PagedData(IEnumerable<FeedArticle> data, long count, int page)
            {
                Data = data;
                Count = count;
                Page = page;
            }

            public IEnumerable<FeedArticle> Data { get; }
            public long Count { get; }
            public int Page { get; }
        }


        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {

            }
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

                    var articles = await session.Query<FeedArticle>()
                        .Stats(out stats)
                         .Where(q => q.Title.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                                || q.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                                || q.Slug.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                        .Skip(request.Skip)
                        .Take(request.Take)
                        .OrderBy(o => o.Title)
                        .ToListAsync();

                    return new PagedData(articles, stats.TotalResults, request.CurrentPage);
                }
            }

        }
    }
}
