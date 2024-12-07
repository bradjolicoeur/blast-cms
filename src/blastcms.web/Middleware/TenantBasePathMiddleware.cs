using blastcms.web.Tenant;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace blastcms.web.Middleware
{
    public class TenantBasePathMiddleware 
    {
        private readonly RequestDelegate _next;

        public TenantBasePathMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var match = Regex.Match(context.Request.Path ,"^(\\/[a-zA-Z0-9_\\-]+)");
            var pathBase = match.Value;
            if (context.Request.Path.StartsWithSegments(pathBase, out var remainder))
            {
                context.Request.Path = remainder;
                context.Request.PathBase = pathBase;
            }

            await _next(context);
        }
    }

    public static class TenatBasePathMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantBasePathMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantBasePathMiddleware>();
        }
    }
}
