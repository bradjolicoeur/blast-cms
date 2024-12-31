using AutoMapper;
using blastcms.web.Data;
using Marten.Linq;
using Marten;
using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System;

namespace blastcms.web.Handlers
{
    public class GetEmailTemplates
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

        public class PagedData : IPagedData<EmailTemplate>
        {
            public PagedData(IEnumerable<EmailTemplate> data, long count, int page)
            {
                Data = data;
                Count = count;
                Page = page;
            }

            public IEnumerable<EmailTemplate> Data { get; }
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

                    var articles = await session.Query<EmailTemplate>()
                        .Stats(out stats)
                         .Where(q => q.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                                || q.Subject.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                        .Skip(request.Skip)
                        .Take(request.Take)
                        .OrderBy(o => o.Name)
                        .ToListAsync();

                    return new PagedData(articles, stats.TotalResults, request.CurrentPage);
                }
            }

        }
    }
}
