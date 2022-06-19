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
    public class ApiAuthorizationHandler
    {
        public class Query : IRequest<Model>
        {
            public Query(string key)
            {
                Key = key;
            }

            public string Key { get; }
        }

        public readonly record struct Model(bool Valid)
        {

        }


        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly ISessionFactory _sessionFactory;
            private readonly IHashingService _hashService;
            private readonly ITenantInfo _tenantInfo;
            private readonly string _key;
            public const string APIKEYNAME = "ApiKey";

            public Handler(ISessionFactory sessionFactory, ITenantInfo tenantInfo, IConfiguration configuration, IHashingService hashService)
            {
                _sessionFactory = sessionFactory;
                _hashService = hashService;

                _tenantInfo = tenantInfo;
                if (_tenantInfo == null) throw new NullReferenceException($"TenantInfo was null");
                _key = configuration.GetValue<string>(APIKEYNAME);
                if (_key == null) throw new NullReferenceException($"{APIKEYNAME} environment variable was not provided");
            }

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                using var session = _sessionFactory.QuerySession();

                var keyHash = _hashService.RegenHash(request.Key);

                //Hash is not coming out the same?
                var data = await session.Query<SecureValue>().FirstOrDefaultAsync(q => q.Id == keyHash && q.Expired == false);

                return new Model(!(data == null));
               
            }

        }
    }
}
