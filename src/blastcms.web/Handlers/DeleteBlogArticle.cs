
namespace blastcms.web.Handlers
{
    using AutoMapper;
    using blastcms.web.Data;
    using Marten;
    using MediatR;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading;
    using System.Threading.Tasks;
    public class DeleteBlogArticle
    {
        public class Command : IRequest<Model>
        {
            [Required]
            public Guid Id { get; set; }

        }

        public readonly record struct Model(bool Success)
        {
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

                using var session = _sessionFactory.OpenSession();

                session.Delete<BlogArticle>(request.Id);
                await session.SaveChangesAsync(cancellationToken);

                return new Model(true);
                
            }

        }
    }
}
