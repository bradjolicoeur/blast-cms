using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using Riok.Mapperly.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.ComponentModel.DataAnnotations;

namespace blastcms.web.Handlers
{
    public partial class AlterEmailTemplate
    {
        public class Command : IRequest<Model>
        {
            [Required]
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


        [Mapper]
        public partial class SliceMapper
        {
            public partial EmailTemplate ToEmailTemplate(Command source);

            public partial Command ToCommand(EmailTemplate source);
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
                var item = Mapper.ToEmailTemplate(request);

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
