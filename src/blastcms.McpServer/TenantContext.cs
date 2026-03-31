namespace blastcms.McpServer;

/// <summary>
/// Scoped service that holds the resolved tenant identifier for the current MCP request.
/// </summary>
public class TenantContext
{
    public string TenantId { get; set; } = string.Empty;
}
