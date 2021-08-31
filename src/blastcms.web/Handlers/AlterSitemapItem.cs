using AutoMapper;
using blastcms.web.Data;
using Marten;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class AlterSitemapItem
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }
            public string RelativePath { get; set; }
            public DateTime? LastModified { get; set; }

        }

        public class Model
        {
            public Model(SitemapItem sitemapItem)
            {
                Data = sitemapItem;
            }

            public SitemapItem Data { get; }
        }


        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Command, SitemapItem>().ReverseMap();
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
                var article = _mapper.Map<SitemapItem>(request);

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
