﻿using AutoMapper;
using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace blastcms.web.Handlers
{
    public class AlterUrlRedirect
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }
            public string RedirectFrom { get; set; }
            public string RedirectTo { get; set; }
            public bool Permanent { get; set; }

        }

        public class Model
        {
            public Model(UrlRedirect urlRedirect)
            {
                Data = urlRedirect;
            }

            public UrlRedirect Data { get; }
        }


        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Command, UrlRedirect>().ReverseMap();
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
                var article = _mapper.Map<UrlRedirect>(request);

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
