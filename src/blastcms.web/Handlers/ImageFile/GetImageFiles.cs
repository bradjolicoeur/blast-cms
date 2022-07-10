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
    public class GetImageFiles
    {
        public class Query : IRequest<PagedData>
        {
            public int Skip { get; internal set; }
            public int Take { get; internal set; }
            public int CurrentPage { get; internal set; }
            public string Search { get; internal set; }

            public string Tag { get; internal set; }

            public Query(int skip, int take, int currentPage, string search = null, string tag = null)
            {
                Skip = skip;
                Take = take;
                CurrentPage = currentPage;
                Search = search;
                Tag = tag;
            }
        }

        public class PagedData : IPagedData<ImageFile>
        {
            public PagedData(IEnumerable<ImageFile> data, long count, int page)
            {
                Data = data;
                Count = count;
                Page = page;
            }

            public IEnumerable<ImageFile> Data { get; }
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

                    var query = session.Query<ImageFile>()
                         .Stats(out stats)

                         .If(!string.IsNullOrWhiteSpace(request.Search), x => x.Where(q => q.Title.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                                 || q.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                                 || q.ImageUrl.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                                 || q.Tags != null && q.Tags.Contains(request.Search)))

                         .If(!string.IsNullOrWhiteSpace(request.Tag), x => x.Where(q => q.Tags != null && q.Tags.Contains(request.Tag)))

                         .Skip(request.Skip)
                         .Take(request.Take)
                         .OrderBy(o => o.Title).AsQueryable();

                    var articles = await query.ToListAsync();

                    return new PagedData(articles, stats.TotalResults, request.CurrentPage);
                }
            }

        }
    }
}

