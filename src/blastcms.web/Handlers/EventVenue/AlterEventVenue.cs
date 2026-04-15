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
    public partial class AlterEventVenue
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }

            [Required]
            public string VenueName { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Zip { get; set; }
            public string Latitude { get; set; }
            public string Longitude { get; set; }
            public string WebsiteUrl { get; set; }
            public ImageFile Image { get; set; }


            [Required]
            public string Slug { get; set; }

        }

        public record Model(EventVenue Data)
        {
        }


        [Mapper]
        public partial class SliceMapper
        {
            public partial EventVenue ToEventVenue(Command source);

            public partial Command ToCommand(EventVenue source);
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
                var item = Mapper.ToEventVenue(request);

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
