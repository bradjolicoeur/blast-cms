using AutoMapper;
using blastcms.web.Data;
using Marten;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class AlterFeedArticle
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }
            public string Slug { get; set; }
            public string Title { get; set; }
            public string ArticleUrl { get; set; }
            public string ImageUrl { get; set; }
            public string Author { get; set; }
            public string Description { get; set; }
            public string Notes { get; set; }
            public string SiteName { get; set; }
            public DateTime DatePosted { get; set; }

        }

        public class Model
        {
            public Model(FeedArticle data)
            {
                Data = data;
            }

            public FeedArticle Data { get; }
        }


        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Command, FeedArticle>().ReverseMap();
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
                var article = _mapper.Map<FeedArticle>(request);

                using var session = _sessionFactory.OpenSession();
                {
                    session.Store(article);

                    await session.SaveChangesAsync();

                    return new Model(article);
                }
            }

        }
    }
}
