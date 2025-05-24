using AutoMapper;
using blastcms.web.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using blastcms.UserManagement.Models;
using blastcms.UserManagement;

namespace blastcms.web.Handlers
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


        public class PagedData : IPagedData<BlastUser>
        {
            public PagedData(IEnumerable<BlastUser> users, long count, int page)
            {
                Data = users;
                Count = count;
                Page = page;
            }

            public IEnumerable<BlastUser> Data { get; }
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

            private readonly IUserManagementProvider _userManagement;

            public Handler(IUserManagementProvider userManagement)
            {
                _userManagement = userManagement;
            }

            public async Task<PagedData> Handle(Query request, CancellationToken cancellationToken)
            {

                var results = await _userManagement.GetAllUsers(request.Skip, request.Take, request.Search);

                return new PagedData(results.Users, results.Total.Value, request.CurrentPage);
            }

        }
    }
}
