using blastcms.web.Data;
using blastcms.web.Infrastructure;
using Marten;
using System;
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
                    var article = await session.Query<BlogArticle>().FirstAsync(q => q.Id == request.Id);

                    return new Model(article);
                }
            }

        }
    }
}
