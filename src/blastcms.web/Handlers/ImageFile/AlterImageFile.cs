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
    public class AlterImageFile
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }
            public string Title { get; set; }
            public IEnumerable<string> Tags { get; set; }
            public string Description { get; set; }
            public string ImageStorageName { get; set; }
            public string ImageUrl { get; set; }

        }

        public class Model
        {
            public Model(ImageFile data)
            {
                Data = data;
            }

            public ImageFile Data { get; }
        }


        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Command, ImageFile>().ReverseMap();
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
                var article = _mapper.Map<ImageFile>(request);

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
