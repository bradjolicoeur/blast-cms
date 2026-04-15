using blastcms.web.Data;
using blastcms.web.Infrastructure;
using Marten;
using Riok.Mapperly.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public partial class AlterBlogArticle
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }
            [Required]
            public string Title { get; set; }
            public string Author { get; set; }
            public IEnumerable<string> Tags { get; set; }

            [Required]
            public DateTime? PublishedDate { get; set; }
            public ImageFile Image { get; set; }
            public string Description { get; set; }
            public string Body { get; set; }

            [Required]
            public string Slug { get; set; }

        }

        public class Model
        {
            public Model(BlogArticle article)
            {
                Article = article;
            }

            public BlogArticle Article { get; }
        }


        [Mapper]
        public partial class SliceMapper
        {
            public partial BlogArticle ToArticle(Command source);

            public partial Command ToCommand(BlogArticle source);
        }

        public class Handler : IRequestHandler<Command, Model>
        {
            private static readonly SliceMapper Mapper = new();
            private readonly ISessionFactory _sessionFactory;

            public Handler(ISessionFactory sessionFactory)
            {
                _sessionFactory = sessionFactory;
            }

            public async Task<Model> Handle(Command request, CancellationToken cancellationToken)
            {
                var article = Mapper.ToArticle(request);

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

