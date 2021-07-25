using AutoMapper;
using blastcms.web.Data;
using Marten;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GetLandingPages
    {
        public class Query : IRequest<Model>
        {
            public Query() { }

            public Query(int take)
            {
                Take = take;
            }

            public int? Take { get; }
        }

        public class Model
        {
            public Model(IEnumerable<LandingPage> data)
            {
                Data = data;
            }

            public IEnumerable<LandingPage> Data { get; }
        }


        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {

            }
        }

        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly ISessionFactory _sessionFactory;
            private readonly IMapper _mapper;

            public Handler(ISessionFactory sessionFactory, IMapper mapper)
            {
                _sessionFactory = sessionFactory;
                _mapper = mapper;
            }

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                using var session = _sessionFactory.QuerySession();
                {
                    var articles = session.Query<LandingPage>().OrderBy(o => o.Title).ToList();

                    return new Model(articles);
                }
            }

        }
    }
}
