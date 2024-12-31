using AutoMapper;
using blastcms.web.Data;
using Marten;
using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.ComponentModel.DataAnnotations;

namespace blastcms.web.Handlers
{
    public class AlterEmailTemplate
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }

            public string FromAddress { get; set; }

            [Required]
            public string Name { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }

        }

        public record Model(EmailTemplate Data)
        {
        }


        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Command, EmailTemplate>().ReverseMap();
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
                var item = _mapper.Map<EmailTemplate>(request);

                using var session = _sessionFactory.OpenSession();
                {
                    session.Store(item);

                    await session.SaveChangesAsync();

                    return new Model(item);
                }
            }

        }
    }
}
