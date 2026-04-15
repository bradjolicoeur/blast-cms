using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using Riok.Mapperly.Abstractions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public partial class AlterEventItem
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }

            [Required]
            public string Title { get; set; }
            public string Body { get; set; }
            public string Summary { get; set; }
            public string Special { get; set; }

            [Required(ErrorMessage = "Venue selection is required")]
            public EventVenue Venue { get; set; }
            public string TicketPrice { get; set; }
            public ImageFile Flyer { get; set; }
            public OpenMicOption OpenMicSignup { get; set; }

            [Required]
            public DateTime? EventDate { get; set; }

            public TimeSpan? EventTime { get; set; }

            public string Sponsor { get; set; }
            public TicketSaleProvider TicketSaleProvider { get; set; }
            public string TicketSaleValue { get; set; }
            public string VenueTicketsUrl { get; set; }

            [Required]
            public string Slug { get; set; }

        }

        public record Model(EventItem Data)
        {
        }


        [Mapper]
        public partial class SliceMapper
        {
            [MapProperty("Venue.Id", nameof(EventItem.VenueId))]
            public partial EventItem ToEventItem(Command source);

            [MapProperty(nameof(EventItem.VenueId), "Venue.Id")]
            public partial Command ToCommand(EventItem source);
        }

        public class Handler : IRequestHandler<Command, Model>
        {
            private static readonly SliceMapper Mapper = new();
            private readonly ISessionFactory _sessionFactory;

            public Handler(ISessionFactory sessionFactory)
            {
                _sessionFactory = sessionFactory;
            }

            public async Task<Model> Handle(Command request, CancellationToken cancellationToken)
            {
                var item = Mapper.ToEventItem(request);

                using var session = _sessionFactory.OpenSession();
                {
                    session.Store(item);

                    await session.SaveChangesAsync();

                    return new Model(item);
                }
            }

        }
    }


}
