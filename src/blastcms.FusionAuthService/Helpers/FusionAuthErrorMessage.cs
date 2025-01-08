using com.inversoft.error;
using System.Text;

namespace blastcms.FusionAuthService.Helpers
{
    public static class LoggingFormatters
    {
        public static string FusionAuthErrorMessage(this Errors error)
        {
            var sb = new StringBuilder();
            foreach (var item in error.generalErrors)
            {
                sb.AppendLine(item.message);
            }
            foreach (var item in error.fieldErrors)
            {
                foreach (var msg in item.Value)
                {
                    sb.AppendLine(msg.message);
                }

            }

            return sb.ToString();
        }
    }
}
