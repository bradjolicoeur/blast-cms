using blastcms.web.Data;
using blastcms.web.Security;
using Finbuckle.MultiTenant;
using Marten;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GenerateApiKeyHandler
    {
        public class Command : IRequest<Model>
        {
            
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
                    Display = newValue.Item2.Substring((newValue.Item2.Length - 3)) 
                });

                await session.SaveChangesAsync();

                return new Model(newValue.Item2);

            }

        }
    }
}
