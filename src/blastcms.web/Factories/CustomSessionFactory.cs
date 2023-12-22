﻿using Finbuckle.MultiTenant;
using Marten;
using System.Data;

namespace blastcms.web.Factories
{
    public class CustomSessionFactory : ISessionFactory
    {
        private readonly IDocumentStore _store;
        private readonly IMultiTenantContextAccessor<TenantInfo> _httpContextAccessor;

        // This is important! You will need to use the
        // IDocumentStore to open sessions
        public CustomSessionFactory(IDocumentStore store, IMultiTenantContextAccessor<TenantInfo> httpContextAccessor)
        {
            _store = store;
            _httpContextAccessor = httpContextAccessor;
        }

        public IQuerySession QuerySession()
        {
            return _store.QuerySession(_httpContextAccessor.MultiTenantContext?.TenantInfo?.Id);
        }

        public IDocumentSession OpenSession()
        {
            // Opting for the "lightweight" session
            // option with no identity map tracking
            // and choosing to use Serializable transactions
            // just to be different
            return _store.LightweightSession(_httpContextAccessor.MultiTenantContext?.TenantInfo?.Id, IsolationLevel.Serializable);
        }
    }
}
