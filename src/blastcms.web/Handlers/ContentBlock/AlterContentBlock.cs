using blastcms.web.Data;
using blastcms.web.Infrastructure;
using Marten;
using Riok.Mapperly.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public partial class AlterContentBlock
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }
            public string Title { get; set; }
            public IEnumerable<string> Groups { get; set; }
            public ImageFile Image { get; set; }
            public string Body { get; set; }
            public string Slug { get; set; }

        }

        public class Model
        {
            public Model(ContentBlock data)
            {
                Data = data;
            }

            public ContentBlock Data { get; }
        }


        [Mapper]
        public partial class SliceMapper
        {
            public partial ContentBlock ToContentBlock(Command source);

            public partial Command ToCommand(ContentBlock source);
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
                var article = Mapper.ToContentBlock(request);

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
