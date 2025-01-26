using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blastcms.FusionAuthService
{
    public interface IFusionAuthTenantProvider
    {
        public string GetTenantId();
        public string GetApplicationId();
    }
}
