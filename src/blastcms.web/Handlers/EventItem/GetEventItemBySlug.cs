using blastcms.web.Data;
using Marten;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GetEventItemBySlug
    {
        public class Query : IRequest<Model>
        {
            public Query(string slug)
            {
                Slug = slug;
            }

            public string Slug { get; }
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

        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly ISessionFactory _sessionFactory;

            public Handler(ISessionFactory sessionFactory)
            {
                _sessionFactory = sessionFactory;
            }

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                using var session = _sessionFactory.QuerySession();
                
                EventVenue venue = null;
                var data = await session.Query<EventItem>()
                    .Include<EventVenue>(x => x.VenueId, x => venue = x)
                    .FirstOrDefaultAsync(q => q.Slug.Equals(request.Slug, StringComparison.OrdinalIgnoreCase), token: cancellationToken);

                return new Model(data, venue);
                
            }

        }
    }
}
