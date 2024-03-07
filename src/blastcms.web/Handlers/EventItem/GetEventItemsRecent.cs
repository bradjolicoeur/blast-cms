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
    public class GetEventItemsRecent
    {
        public class Query : IRequest<PagedData>
        {
            public int Skip { get; internal set; }
            public int Take { get; internal set; }
            public int CurrentPage { get; internal set; }
            public string Search { get; internal set; }

            public int? Days { get; internal set; }

            public string Tag { get; internal set; }

            public Query(int skip, int take, int currentPage, string search = null, string tag = null, int? days = 30)
            {
                Skip = skip;
                Take = take;
                CurrentPage = currentPage;
                Search = search;
                Tag = tag;
                Days = days;
            }
        }


        public class PagedData : IPagedData<EventItemModel>
        {
            public PagedData(IEnumerable<EventItemModel> data, long count, int page)
            {
                Data = data;
                Count = count;
                Page = page;
            }

            public IEnumerable<EventItemModel> Data { get; }
            public long Count { get; }
            public int Page { get; }
        }

        public record EventItemModel(EventItem Event, EventVenue Venue)
        {
           
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

                var dict = new Dictionary<Guid, EventVenue>();

                var query = session.Query<EventItem>()
                    .Stats(out QueryStatistics stats)
                    .Include(x => x.VenueId, dict)

                    .If(request.Days.HasValue, x => x.Where(q => q.EventDate > DateTime.UtcNow.AddDays(-request.Days.Value)))

                    .If(!string.IsNullOrWhiteSpace(request.Search), x => x.Where(q => q.Title.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                            || q.Slug.Contains(request.Search, StringComparison.OrdinalIgnoreCase)))

                    .Skip(request.Skip)
                    .Take(request.Take)
                    .OrderByDescending(o => o.EventDate).AsQueryable();

                var data = await query
                    .ToListAsync(token: cancellationToken);

                var venueList = dict.Values.ToList();

                var merged = data.Select(s => new EventItemModel(s, venueList.Where(q => q.Id==s.VenueId).FirstOrDefault())).ToList();

                return new PagedData(merged, stats.TotalResults, request.CurrentPage);
                
            }

        }
    }
}
