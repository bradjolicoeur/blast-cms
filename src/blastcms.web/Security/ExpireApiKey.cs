using blastcms.web.Data;
using Marten;
using blastcms.web.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class ExpireApiKey
    {
        public record Command(string Id) : IRequest<Model>
        {

        }

        public record Model(bool Success)
        {

        }

        public class Handler : IRequestHandler<Command, Model>
        {
            private readonly ISessionFactory _sessionFactory;

            public Handler(ISessionFactory sessionFactory)
            {
                _sessionFactory = sessionFactory;
            }

            public async Task<Model> Handle(Command request, CancellationToken cancellationToken)
            {
               

                using var session = _sessionFactory.OpenSession();

                var data = await session.LoadAsync<SecureValue>(request.Id, cancellationToken);

                data.Expired = true;

                session.Store(data);

                await session.SaveChangesAsync(cancellationToken);

                return new Model(true);
                
            }

        }
    }
}
