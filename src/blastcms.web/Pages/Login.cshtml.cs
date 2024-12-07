using System.Threading.Tasks;
using blastcms.web.Tenant;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace blastcms.web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly TenantBasePath _tenantInfo;

        public LoginModel(TenantBasePath tenantInfo)
        {
            _tenantInfo = tenantInfo;
        }
        public async Task OnGet(string redirectUri = "/")
        {
            redirectUri = _tenantInfo.BasePath ?? "/";
            await HttpContext.ChallengeAsync("OpenIdConnect", new AuthenticationProperties
            {
                RedirectUri = redirectUri
            });
        }
    }
}
