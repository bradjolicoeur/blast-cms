using AutoMapper;
using blastcms.ArticleScanService;
using blastcms.web.Data;
using Marten;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class ScanAndAddUrlToFeed
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

        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<MetaInformation, FeedArticle>()
                    .ForMember(dest => dest.DatePosted, opt => opt.MapFrom(src => DateTime.UtcNow));
            }
        }

        public class Handler : IRequestHandler<Command, Model>
        {
            private readonly IMetaScraper _metaScraper;
            private readonly ISessionFactory _sessionFactory;
            private readonly IMapper _mapper;

            public Handler(IMetaScraper metaScraper, ISessionFactory sessionFactory, IMapper mapper)
            {
                _metaScraper = metaScraper;
                _sessionFactory = sessionFactory;
                _mapper = mapper;
            }

            public async Task<Model> Handle(Command request, CancellationToken cancellationToken)
            {
                var meta = _metaScraper.GetMetaDataFromUrl(request.UrlToScan);

                var feedArticle = _mapper.Map<FeedArticle>(meta);

                using var session = _sessionFactory.OpenSession();
                {
                    session.Store(feedArticle);

                    session.SaveChanges();

                }

                return new Model { FeedArticle = feedArticle };
            }
        }
    }
}