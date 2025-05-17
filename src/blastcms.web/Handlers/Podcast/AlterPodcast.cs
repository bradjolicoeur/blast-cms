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
    public class AlterPodcast
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }

            [Required]
            public string Title { get; set; }
            public string Description { get; set; }

            [Required]
            public DateTime? PublishedDate { get; set; }
            public string PodcastUrl { get; set; }
            public string RssCategory { get; set; }
            public string RssSubcategory { get; set; }
            public ImageFile CoverImage { get; set; }
            public string OwnerName { get; set; }
            public string OwnerEmail { get; set; }
            public bool ExplicitContent { get; set; }

            [Required]
            public string Slug { get; set; }

        }

        public record Model(Podcast Podcast)
        {
        }


        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Command, Podcast>().ReverseMap();
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
                var item = _mapper.Map<Podcast>(request);

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
