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
    public partial class AlterPodcast
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


        [Mapper]
        public partial class SliceMapper
        {
            public partial Podcast ToPodcast(Command source);

            public partial Command ToCommand(Podcast source);
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
                var item = Mapper.ToPodcast(request);

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
