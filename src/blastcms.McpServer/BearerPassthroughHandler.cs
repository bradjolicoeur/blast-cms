using System.Net.Http.Headers;

namespace blastcms.McpServer;

public class BearerPassthroughHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = httpContextAccessor.HttpContext;
        if (context is not null &&
            context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var header = authHeader.ToString();
            if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = header["Bearer ".Length..];
                request.Headers.Remove("ApiKey");
                request.Headers.Add("ApiKey", token);
            }
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
