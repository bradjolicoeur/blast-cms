using AutoMapper;
using blastcms.web.Data;
using Marten.Linq;
using Marten;
using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace blastcms.web.Handlers.UserManagement
{
    public class GetUsers
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


        public class PagedData : IPagedData<BlogArticle>
        {
            public PagedData(IEnumerable<BlogArticle> articles, long count, int page)
            {
                Data = articles;
                Count = count;
                Page = page;
            }

            public IEnumerable<BlogArticle> Data { get; }
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

            private readonly IMapper _mapper;

            public Handler(IMapper mapper)
            {
                _mapper = mapper;
            }

            public async Task<PagedData> Handle(Query request, CancellationToken cancellationToken)
            {

                throw new NotImplementedException();
            }

        }
    }
}
