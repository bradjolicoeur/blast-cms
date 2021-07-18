using AutoMapper;
using Marten;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GetBlogArticles
    {
        public class Query : IRequest<Model>
        {
            public Query() { }

            public Query( int take)
            {
                Take = take;
            }

            public int Take { get; } = 10;
        }

        public class Model
        {
        }


        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {

            }
        }

        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly ISessionFactory _client;
            private readonly IMapper _mapper;

            //public Handler(ISessionFactory client, IMapper mapper)
            //{
            //    _client = client;
            //    _mapper = mapper;
            //}

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

        }
    }
}
