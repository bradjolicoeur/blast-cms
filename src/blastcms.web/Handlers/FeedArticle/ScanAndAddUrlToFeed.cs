using blastcms.ArticleScanService;
using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using Riok.Mapperly.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public partial class ScanAndAddUrlToFeed
    {
        public class Command : IRequest<Model>
        {
            public string UrlToScan { get; internal set; }

            public Command(string urlToScan)
            {
                UrlToScan = urlToScan;
            }
        }

        public class Model
        {
            public FeedArticle FeedArticle { get; set; }
        }

        [Mapper]
        public partial class SliceMapper
        {
            [MapProperty(nameof(MetaInformation.Keywords), nameof(FeedArticle.KeyWords))]
            [MapperIgnoreTarget(nameof(FeedArticle.Id))]
            [MapperIgnoreTarget(nameof(FeedArticle.Slug))]
            [MapperIgnoreTarget(nameof(FeedArticle.Tags))]
            [MapperIgnoreTarget(nameof(FeedArticle.Notes))]
            [MapperIgnoreTarget(nameof(FeedArticle.DatePosted))]
            public partial FeedArticle ToFeedArticle(MetaInformation source);
        }

        public class Handler : IRequestHandler<Command, Model>
        {
            private static readonly SliceMapper Mapper = new();
            private readonly IMetaScraper _metaScraper;
            private readonly ISessionFactory _sessionFactory;

            public Handler(IMetaScraper metaScraper, ISessionFactory sessionFactory)
            {
                _metaScraper = metaScraper;
                _sessionFactory = sessionFactory;
            }

            public async Task<Model> Handle(Command request, CancellationToken cancellationToken)
            {
                var meta = await _metaScraper.GetMetaDataFromUrl(request.UrlToScan);

                var feedArticle = Mapper.ToFeedArticle(meta);
                feedArticle.DatePosted = DateTime.UtcNow;

                using var session = _sessionFactory.OpenSession();
                {
                    session.Store(feedArticle);

                    await session.SaveChangesAsync();

                }

                return new Model { FeedArticle = feedArticle };
            }
        }
    }
}
