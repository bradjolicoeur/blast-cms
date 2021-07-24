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
    public class GetBlogArticle
    {
        public class Query : IRequest<Model>
        {
            public Query(Guid id) 
            {
                Id = id;
            }

            public Guid Id { get; }
        }

        public class Model
        {
            public Model(BlogArticle article)
            {
                Article = article;
            }
            public BlogArticle Article { get; }
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
                    var article = session.Query<BlogArticle>().First(q => q.Id == request.Id);

                    return new Model(article);
                }
            }

        }
    }
}
