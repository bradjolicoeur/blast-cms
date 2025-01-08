using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blastcms.FusionAuthService.Exceptions
{
    public class FusionAuthException : Exception
    {
        public FusionAuthException(string message): base(message) { }
    }
}
