using blastcms.web.Data;
using blastcms.web.Tenant;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Marten;
using blastcms.web.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

        public readonly record struct Model(bool Valid, bool ro)
        {

        }


        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly ISessionFactory _sessionFactory;
            private readonly IHashingService _hashService;
            private readonly ILogger<Handler> _logger;
            private readonly IMultiTenantContextAccessor<CustomTenantInfo> _httpContextAccessor;
            private readonly string _key;
            public const string APIKEYNAME = "ApiKey";

            public Handler(ISessionFactory sessionFactory, IMultiTenantContextAccessor<CustomTenantInfo> httpContextAccessor, 
                IConfiguration configuration, IHashingService hashService,
                ILogger<Handler> logger)
            {
                _sessionFactory = sessionFactory;
                _hashService = hashService;
                _logger = logger;

                _httpContextAccessor = httpContextAccessor;
                var tenantInfo = _httpContextAccessor.MultiTenantContext?.TenantInfo?.Identifier;

                if (tenantInfo == null) throw new NullReferenceException($"TenantInfo was null");
                _key = configuration.GetValue<string>(APIKEYNAME);
                if (_key == null) throw new NullReferenceException($"{APIKEYNAME} environment variable was not provided");
            }

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    using var session = _sessionFactory.QuerySession();

                    var keyHash = _hashService.RegenHash(request.Key);

                    var data = await session.Query<SecureValue>().FirstOrDefaultAsync(q => q.Id == keyHash && q.Expired == false, token: cancellationToken);

                    return new Model(!(data == null), data.Readonly);

                } catch (Exception ex)
                {
                    _logger.LogError(ex,"Exception with validating API Key");
                    return new Model(false, true);
                }
               
               
            }

        }
    }
}
