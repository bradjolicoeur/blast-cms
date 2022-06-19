using blastcms.web.Data;
using Finbuckle.MultiTenant;
using Marten;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Security
{
    public class GenerateApiKeyHandler
    {
        public class Command : IRequest<Model>
        {
            
        }

        public readonly record struct Model(string key)
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

                session.Store(new SecureValue { Id = newValue.Item1, Expired = false });

                await session.SaveChangesAsync();

                return new Model(newValue.Item2);

            }

        }
    }
}
