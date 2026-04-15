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
    public partial class PutEventItem
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }

            [Required]
            public string Title { get; set; }
            public string Body { get; set; }
            public string Summary { get; set; }
            public string Special { get; set; }

            public EventVenue Venue { get; set; }
            public string TicketPrice { get; set; }
            public ImageFile Flyer { get; set; }
            public string OpenMicSignup { get; set; }

            [Required]
            public DateTime? EventDate { get; set; }

            public TimeSpan? EventTime { get; set; }

            public string Sponsor { get; set; }

            [Required]
            public string TicketSaleProvider { get; set; }
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

            private TicketSaleProvider MapTicketSaleProvider(string source) => TicketSaleProvider.FromName(source);

            private OpenMicOption MapOpenMicSignup(string source) => OpenMicOption.FromName(source);
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
