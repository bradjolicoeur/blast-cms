using blastcms.web.Data;
using blastcms.web.Security;
using Marten;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GenerateApiKeyHandler
    {
        public class Command : IRequest<Model>
        {
            public bool Readonly { get; set; } = true;
        }

        public readonly record struct Model(string Key)
        {

        }


        public class Handler : IRequestHandler<Command, Model>
        {
            private readonly ISessionFactory _sessionFactory;
            private readonly IHashingService _hashingService;


            public Handler(ISessionFactory sessionFactory, IHashingService hashingService)
            {
                _sessionFactory = sessionFactory;
                _hashingService = hashingService;
            }

            public async Task<Model> Handle(Command request, CancellationToken cancellationToken)
            {
                using var session = _sessionFactory.OpenSession();

                var newValue = _hashingService.GenerateNewKey();

                session.Store(new SecureValue { 
                    Id = newValue.Item1, 
                    Expired = false, 
                    Display = newValue.Item2.Substring(0,4),
                    Readonly = request.Readonly,
                    Created = DateTime.UtcNow
                });

                await session.SaveChangesAsync();

                return new Model(newValue.Item2);

            }

        }
    }
}
