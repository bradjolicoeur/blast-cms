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
            private readonly ITenantInfo _tenantInfo;
            private readonly string _key;
            public const string APIKEYNAME = "ApiKey";

            public Handler(ISessionFactory sessionFactory, ITenantInfo tenantInfo, IConfiguration configuration)
            {
                _sessionFactory = sessionFactory;

                _tenantInfo = tenantInfo;
                if (_tenantInfo == null) throw new NullReferenceException($"TenantInfo was null");
                _key = configuration.GetValue<string>(APIKEYNAME);
                if (_key == null) throw new NullReferenceException($"{APIKEYNAME} environment variable was not provided");
            }

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                //using var session = _sessionFactory.QuerySession();
                //{
                //    var data = await session.Query<UrlRedirect>().FirstAsync(q => q.Id == request.Key);

                //    return new Model(data);
                //}

                return new Model(_key.Equals(request.Key));
               
            }

        }
    }
}
