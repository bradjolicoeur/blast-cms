using AutoMapper;
using blastcms.web.Data;
using Marten;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class AlterEventVenue
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
            public ImageFile? Image { get; set; }


            [Required]
            public string Slug { get; set; }

        }

        public record Model(EventVenue Data)
        {
        }


        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Command, EventVenue>().ReverseMap();
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
                var item = _mapper.Map<EventVenue>(request);

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
