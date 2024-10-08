using AutoMapper;
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
    public class GetTenants
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

        public class PagedData : IPagedData<BlastTenant>
        {
            public PagedData(IEnumerable<BlastTenant> tenants, long count, int page)
            {
                Data = tenants;
                Count = count;
                Page = page;
            }

            public IEnumerable<BlastTenant> Data { get; }
            public long Count { get; }
            public int Page { get; }
        }


        public class Handler : IRequestHandler<Query, PagedData>
        {
            private readonly IDocumentStore _documentStore;

            public Handler(IDocumentStore documentStore)
            {
                _documentStore = documentStore;
            }

            public async Task<PagedData> Handle(Query request, CancellationToken cancellationToken)
            {
                using var session = _documentStore.QuerySession();
                {
                    QueryStatistics stats = null;

                    var query = session.Query<BlastTenant>()
                        .Stats(out stats)

                        .If(!string.IsNullOrWhiteSpace(request.Search), x => x.Where(q => q.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                                || q.Identifier.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                                ))

                        .Skip(request.Skip)
                        .Take(request.Take)
                        .OrderByDescending(o => o.Name).AsQueryable();

                    var tenants = await query.ToListAsync();

                    return new PagedData(tenants, stats.TotalResults, request.CurrentPage);
                }
            }

        }
    }
}
