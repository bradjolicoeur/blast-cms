using Microsoft.Extensions.Configuration;

namespace blastcms.web.Data
{
    public class PaddleConfiguration
    {
        public string BillingUrl {  get;  }
        public PaddleConfiguration(IConfiguration configuration)
        {
            BillingUrl = configuration["PaddleBillingUrl"];
        }
    }
}
