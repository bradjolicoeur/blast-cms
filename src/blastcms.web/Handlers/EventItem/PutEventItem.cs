using AutoMapper;
using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class PutEventItem
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


        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Command, EventItem>()
                    .ForMember(dest => dest.VenueId, opt => opt.MapFrom(src => src.Venue.Id))
                    .ForMember(dest => dest.TicketSaleProvider, opt => opt.MapFrom(o => TicketSaleProvider.FromName(o.TicketSaleProvider)))
                    .ForMember(dest => dest.OpenMicSignup, opt => opt.MapFrom(o => OpenMicOption.FromName(o.OpenMicSignup)));
            }

        }

        public class Handler : IRequestHandler<Command, Model>
        {
            private readonly ISessionFactory _sessionFactory;
            private readonly IMapper _mapper;

            public Handler(ISessionFactory sessionFactory, IMapper mapper)
            {
                _sessionFactory = sessionFactory;
                _mapper = mapper;
            }

            public async Task<Model> Handle(Command request, CancellationToken cancellationToken)
            {
                var item = _mapper.Map<EventItem>(request);

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
