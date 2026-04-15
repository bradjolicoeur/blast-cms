using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using Riok.Mapperly.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public partial class AlterLandingPage
    {
        public class Command : IRequest<Model>
        {
            public Guid? Id { get; set; }
            public string Title { get; set; }
            public IEnumerable<string> Tags { get; set; }
            public string Description { get; set; }
            public string HeroTitle { get; set; }
            public string HeroPhrase { get; set; }
            public string HeroImageUrl { get; set; }
            public string Body { get; set; }
            public string Slug { get; set; }

        }

        public class Model
        {
            public Model(LandingPage data)
            {
                Data = data;
            }

            public LandingPage Data { get; }
        }


        [Mapper]
        public partial class SliceMapper
        {
            public partial LandingPage ToLandingPage(Command source);

            public partial Command ToCommand(LandingPage source);
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
                var article = Mapper.ToLandingPage(request);

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
