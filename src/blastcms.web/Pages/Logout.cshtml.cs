using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace blastcms.web.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;
        private readonly IConfiguration _configuration;

        public LogoutModel(ILogger<LogoutModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult OnGet()
        {
            SignOut("cookie", "oidc");
            var host = $"https://{_configuration["FusionAuthSettings:Domain"]}";
            var cookieName = _configuration["FusionAuthSettings:CookieName"];

            var clientId = _configuration["FusionAuthSettings:ClientId"];
            var url = host + "/oauth2/logout?client_id=" + clientId;
            Response.Cookies.Delete(cookieName);
            return Redirect(url);
        }
    }
}
