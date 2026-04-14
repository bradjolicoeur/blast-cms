using Microsoft.AspNetCore.Http;

namespace blastcms.McpServer;

/// <summary>
/// Holds the resolved tenant identifier for the current MCP request.
/// </summary>
public class TenantContext
{
    private static readonly object HttpContextItemKey = new();
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private string _tenantId = string.Empty;

    public TenantContext()
    {
    }

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string TenantId
    {
        get
        {
            if (_httpContextAccessor?.HttpContext?.Items.TryGetValue(HttpContextItemKey, out var tenantId) == true &&
                tenantId is string value)
            {
                return value;
            }

            return _tenantId;
        }
        set
        {
            if (_httpContextAccessor?.HttpContext is { } httpContext)
            {
                httpContext.Items[HttpContextItemKey] = value;
                return;
            }

            _tenantId = value;
        }
    }
}
