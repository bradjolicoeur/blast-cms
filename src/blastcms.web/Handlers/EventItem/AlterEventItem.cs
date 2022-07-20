using AutoMapper;
using blastcms.web.Data;
using Marten;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class AlterEventItem
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
            public HashSet<Guid?> VenueId { get; set; }
            public string TicketPrice { get; set; }
            public ImageFile Flyer { get; set; }
            public string OpenMicSignup { get; set; }

            [Required]
            public DateTime EventDate { get; set; }
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


        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Command, EventItem>()
                    .ForMember(dest => dest.VenueId, opt => opt.MapFrom(src => src.VenueId.First()));

                CreateMap<EventItem, Command>()
                    .ForMember(dest => dest.VenueId, opt => opt.MapFrom(src => new HashSet<Guid?>(new List<Guid?> { src.VenueId })));
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
