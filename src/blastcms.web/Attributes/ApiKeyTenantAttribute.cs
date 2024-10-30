using blastcms.web.Tenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace blastcms.web.Attributes
{
    [AttributeUsage(validOn: AttributeTargets.Method | AttributeTargets.Class)]
    public class ApiKeyTenantAttribute : Attribute, IAsyncActionFilter
    {
        public const string APIKEYNAME = "ApiKey";
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = "Api Key was not provided"
                };
                return;
            }

            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var httpContextAccessor = context.HttpContext.RequestServices.GetRequiredService<IMultiTenantContextAccessor<CustomTenantInfo>>();
            var authorized = extractedApiKey == configuration["TenantAdminKey"] && 
                        (httpContextAccessor.MultiTenantContext?.TenantInfo?.AdminTenant ?? false); 

            if (!authorized)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = "Api Key is not valid"
                };
                return;
            }

            await next();
        }
    }
}
