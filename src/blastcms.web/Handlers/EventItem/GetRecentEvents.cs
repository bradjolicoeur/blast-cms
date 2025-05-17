using blastcms.web.Data;
using blastcms.web.Helpers;
using Marten;
using Marten.Linq;
using blastcms.web.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GetRecentEvents
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


        public class PagedData : IPagedData<Model>
        {
            public PagedData(IEnumerable<Model> data, long count, int page)
            {
                Data = data;
                Count = count;
                Page = page;
            }

            public IEnumerable<Model> Data { get; }
            public long Count { get; }
            public int Page { get; }
        }

        public class Model
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }
            public string Summary { get; set; }
            public string Special { get; set; }
            public Guid VenueId { get; set; }
            public string TicketPrice { get; set; }
            public ImageFile Flyer { get; set; }
            public OpenMicOption OpenMicSignup { get; set; }
            public DateTime? EventDate { get; set; }
            public TimeSpan? EventTime { get; set; }
            public string Sponsor { get; set; }
            public TicketSaleProvider TicketSaleProvider { get; set; }
            public string TicketSaleValue { get; set; }
            public string VenueTicketsUrl { get; set; }
            public string Slug { get; set; }
            public EventVenue Venue { get; private set; }
            public Model(EventItem @event, EventVenue venue)
            {

                Venue = venue;
                Id = @event.Id;
                Title = @event.Title;
                Body = @event.Body;
                Summary = @event.Summary;
                Special = @event.Special;
                VenueId = @event.VenueId;
                TicketPrice = @event.TicketPrice;
                Flyer = @event.Flyer;
                OpenMicSignup = @event.OpenMicSignup;
                EventDate = @event.EventDate;
                EventTime = @event.EventTime;
                Sponsor = @event.Sponsor;
                TicketSaleProvider = @event.TicketSaleProvider;
                TicketSaleValue = @event.TicketSaleValue;
                VenueTicketsUrl = @event.VenueTicketsUrl;
                Slug = @event.Slug;

            }
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

                    .If(request.Days.HasValue, x => x.Where(q => q.EventDate > DateTime.Now.AddDays(-request.Days.Value)))

                    .If(!string.IsNullOrWhiteSpace(request.Search), x => x.Where(q => q.Title.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                            || q.Slug.Contains(request.Search, StringComparison.OrdinalIgnoreCase)))

                    .Skip(request.Skip)
                    .Take(request.Take)
                    .OrderByDescending(o => o.EventDate).AsQueryable();

                var data = await query
                    .ToListAsync(token: cancellationToken);

                var venueList = dict.Values.ToList();

                var merged = data.Select(s => new Model(s, venueList.Where(q => q.Id==s.VenueId).FirstOrDefault())).ToList();

                return new PagedData(merged, stats.TotalResults, request.CurrentPage);
                
            }

        }
    }
}
