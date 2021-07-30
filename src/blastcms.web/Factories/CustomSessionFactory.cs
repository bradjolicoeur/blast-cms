using Finbuckle.MultiTenant;
using Marten;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace blastcms.web.Factories
{
    public class CustomSessionFactory : ISessionFactory
    {
        private readonly IDocumentStore _store;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // This is important! You will need to use the
        // IDocumentStore to open sessions
        public CustomSessionFactory(IDocumentStore store, IHttpContextAccessor httpContextAccessor)
        {
            _store = store;
            _httpContextAccessor = httpContextAccessor;
        }

        public IQuerySession QuerySession()
        {
            return _store.QuerySession(_httpContextAccessor.HttpContext.GetMultiTenantContext<TenantInfo>()?.TenantInfo?.Id);
        }

        public IDocumentSession OpenSession()
        {
            // Opting for the "lightweight" session
            // option with no identity map tracking
            // and choosing to use Serializable transactions
            // just to be different
            return _store.LightweightSession(_httpContextAccessor.HttpContext.GetMultiTenantContext<TenantInfo>()?.TenantInfo?.Id, IsolationLevel.Serializable);
        }
    }
}
