using AutoMapper;
using blastcms.web.Data;
using Marten;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GetBlogArticleBySlug
    {
        public class Query : IRequest<Model>
        {
            public Query(string slug) 
            {
                Slug = slug;
            }

            public string Slug { get; }
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

            public Handler(ISessionFactory sessionFactory)
            {
                _sessionFactory = sessionFactory;
            }

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                using var session = _sessionFactory.QuerySession();
                {
                    var article = await session.Query<BlogArticle>().FirstAsync(q => q.Slug.Equals(request.Slug, StringComparison.OrdinalIgnoreCase));

                    return new Model(article);
                }
            }

        }
    }
}
