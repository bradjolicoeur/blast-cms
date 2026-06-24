using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using Riok.Mapperly.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace blastcms.web.Handlers
{
    public partial class AlterCtaConfiguration
    {
        public class Command : IRequest<Model>
        {
            [Required]
            public Guid? Id { get; set; }

            [Required]
            public string Name { get; set; }

            [Required]
            public string Slug { get; set; }

            public string FromEmail { get; set; }
            public string AdminEmail { get; set; }
            public Guid? AdminEmailTemplateId { get; set; }
            public Guid? SubmitterEmailTemplateId { get; set; }
        }

        public record Model(CtaConfiguration Data);

        [Mapper]
        public partial class SliceMapper
        {
            public partial CtaConfiguration ToCtaConfiguration(Command source);
            public partial Command ToCommand(CtaConfiguration source);
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
                var item = Mapper.ToCtaConfiguration(request);

                using var session = _sessionFactory.OpenSession();
                session.Store(item);
                await session.SaveChangesAsync(cancellationToken);

                return new Model(item);
            }
        }
    }
}
