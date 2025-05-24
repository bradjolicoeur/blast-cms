using AutoMapper;
using blastcms.web.Data;
using FluentValidation;
using Marten;
using blastcms.web.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class AlterPodcastEpisode
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }
            
            [Required(ErrorMessage = "Podcast selection is required")]
            public IEnumerable<Guid?> PodcastId { get; set; }

            [Required]
            public string Title { get; set; }
            public string Author { get; set; }
            public IEnumerable<string> Tags { get; set; }

            [Required]
            public DateTime? PublishedDate { get; set; }
            public ImageFile Image { get; set; }
            public string Summary { get; set; }
            public string Content { get; set; }
            public int Episode { get; set; }
            public string Duration { get; set; }
            public string Mp3Url { get; set; }
            public string YouTubeUrl { get; set; }

            [Required]
            public string Slug { get; set; }

        }

        public record Model(PodcastEpisode Article)
        {
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Title).NotNull();
                RuleFor(x => x.PodcastId).NotEmpty();
                RuleFor(x => x.Slug).NotNull();
            }
        }


        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Command, PodcastEpisode>()
                    .ForMember(dest => dest.PodcastId, opt => opt.MapFrom(src => src.PodcastId.First()));

                CreateMap<PodcastEpisode, Command>()
                    .ForMember(dest => dest.PodcastId, opt => opt.MapFrom(src => new HashSet<Guid?>(new List<Guid?> { src.PodcastId })));
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
                var item = _mapper.Map<PodcastEpisode>(request);

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
