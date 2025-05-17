using AutoMapper;
using blastcms.web.Data;
using blastcms.web.Infrastructure;
using Marten;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class AlterBlogArticle
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


        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Command, BlogArticle>().ReverseMap();
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
                var article = _mapper.Map<BlogArticle>(request);

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

