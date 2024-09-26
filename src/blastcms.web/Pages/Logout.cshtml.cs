using blastcms.web.Tenant;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
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
        private readonly IMultiTenantContextAccessor<CustomTenantInfo> _httpContextAccessor;

        public LogoutModel(ILogger<LogoutModel> logger, IConfiguration configuration, IMultiTenantContextAccessor<CustomTenantInfo> httpContextAccessor)
        {
            _logger = logger;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult OnGet()
        {
            var tenantInfo = _httpContextAccessor.MultiTenantContext.TenantInfo;

            Request.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Request.HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
   
            var url = tenantInfo.OpenIdConnectAuthority + "/oauth2/logout?client_id=" + tenantInfo.OpenIdConnectClientId;
            return Redirect(url);
        }
    }
}
