using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using Riok.Mapperly.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace blastcms.web.Handlers
{
    public partial class AlterContentTag
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }
            public string Value { get; set; }

        }

        public class Model
        {
            public Model(ContentTag data)
            {
                Data = data;
            }

            public ContentTag Data { get; }
        }


        [Mapper]
        public partial class SliceMapper
        {
            public partial ContentTag ToContentTag(Command source);

            public partial Command ToCommand(ContentTag source);
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
                var article = Mapper.ToContentTag(request);

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
