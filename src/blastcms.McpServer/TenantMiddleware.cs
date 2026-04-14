namespace blastcms.McpServer;

/// <summary>
/// Extracts the tenant identifier from the first URL path segment when the path matches
/// /{tenant}/mcp[/...], rewrites the path to /mcp[/...], and populates TenantContext.
/// Returns 400 if the client connects directly to /mcp without a tenant prefix.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Reject direct /mcp access (no tenant)
        if (path.Equals("/mcp", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/mcp/", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Tenant identifier is required. Use /{tenant}/mcp.");
            return;
        }

        // Match /{tenant}/mcp[/...]
        var trimmed = path.TrimStart('/');
        var parts = trimmed.Split('/', 2);

        if (parts.Length == 2 &&
            (parts[1].Equals("mcp", StringComparison.OrdinalIgnoreCase) ||
             parts[1].StartsWith("mcp/", StringComparison.OrdinalIgnoreCase)))
        {
            tenantContext.TenantId = parts[0];
            context.Request.Path = "/" + parts[1];
        }

        await _next(context);
    }
}
