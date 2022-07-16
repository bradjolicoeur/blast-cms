using AutoMapper;
using blastcms.web.Data;
using Marten;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class AlterLandingPage
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }
            public string Title { get; set; }
            public HashSet<String> Tags { get; set; }
            public string Description { get; set; }
            public string HeroTitle { get; set; }
            public string HeroPhrase { get; set; }
            public string HeroImageUrl { get; set; }
            public string Body { get; set; }
            public string Slug { get; set; }

        }

        public class Model
        {
            public Model(LandingPage data)
            {
                Data = data;
            }

            public LandingPage Data { get; }
        }


        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Command, LandingPage>().ReverseMap();
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
                var article = _mapper.Map<LandingPage>(request);

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
