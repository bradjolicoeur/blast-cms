using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace blastcms.McpServer.Tools;

/// <summary>
/// MCP tools for managing URL Redirects in Blast CMS.
/// URL Redirects allow mapping old URLs to new destinations for SEO and link management.
/// </summary>
[McpServerToolType]
public class UrlRedirectTools
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TenantContext _tenantContext;

    public UrlRedirectTools(IHttpClientFactory httpClientFactory, TenantContext tenantContext)
    {
        _httpClientFactory = httpClientFactory;
        _tenantContext = tenantContext;
    }

    [McpServerTool]
    [Description("Retrieves a paginated list of URL redirects from Blast CMS.")]
    public async Task<string> ListUrlRedirects(
        [Description("Page number to retrieve, starting at 1")] int page = 1,
        [Description("Number of URL redirects per page (default is 10)")] int pageSize = 10)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var url = $"{_tenantContext.TenantId}/api/urlredirect/all?currentPage={page}&take={pageSize}&skip=0";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single URL redirect by its source URL path from Blast CMS.")]
    public async Task<string> GetUrlRedirectByFrom(
        [Description("The source relative URL path to look up (e.g. '/old-page')")] string fromUrl)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"{_tenantContext.TenantId}/api/urlredirect/from/{Uri.EscapeDataString(fromUrl)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
