using blastcms.web.Security;
using blastcms.web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace blastcms.web.Attributes
{
    [AttributeUsage(validOn: AttributeTargets.Method | AttributeTargets.Class)]
    public class ApiKeyFullAttribute : Attribute, IAsyncActionFilter
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

            var mediator = context.HttpContext.RequestServices.GetRequiredService<IDispatcher>();
            var authorized = await mediator.Send(new ApiAuthorizationHandler.Query(extractedApiKey));

            if (!authorized.Valid)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = "Api Key is not valid"
                };
                return;
            }

            if (authorized.Valid && authorized.ro)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = "Api Key is readonly"
                };
                return;
            }

            await next();
        }
    }
}
