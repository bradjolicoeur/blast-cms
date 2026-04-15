using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using Riok.Mapperly.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace blastcms.web.Handlers
{
    public partial class AlterUrlRedirect
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


        [Mapper]
        public partial class SliceMapper
        {
            public partial UrlRedirect ToUrlRedirect(Command source);

            public partial Command ToCommand(UrlRedirect source);
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
                var article = Mapper.ToUrlRedirect(request);

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
