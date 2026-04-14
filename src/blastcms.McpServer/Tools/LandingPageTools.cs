using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace blastcms.McpServer.Tools;

/// <summary>
/// MCP tools for managing Landing Pages in Blast CMS.
/// Landing Pages are standalone pages designed for specific campaigns or purposes.
/// </summary>
[McpServerToolType]
public class LandingPageTools
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TenantContext _tenantContext;

    public LandingPageTools(IHttpClientFactory httpClientFactory, TenantContext tenantContext)
    {
        _httpClientFactory = httpClientFactory;
        _tenantContext = tenantContext;
    }

    [McpServerTool]
    [Description("Retrieves a paginated list of landing pages from Blast CMS. Supports optional search filtering.")]
    public async Task<string> ListLandingPages(
        [Description("Optional search term to filter landing pages by title or content")] string? search = null,
        [Description("Page number to retrieve, starting at 1")] int page = 1,
        [Description("Number of landing pages per page (default is 10)")] int pageSize = 10)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var url = $"{_tenantContext.TenantId}/api/landingpage/all?currentPage={page}&take={pageSize}&skip=0";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single landing page by its URL slug from Blast CMS. The slug is the human-readable identifier used in the page URL.")]
    public async Task<string> GetLandingPageBySlug(
        [Description("The URL slug of the landing page (e.g. 'summer-sale-2024')")] string slug)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"{_tenantContext.TenantId}/api/landingpage/slug/{Uri.EscapeDataString(slug)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single landing page by its unique identifier (GUID) from Blast CMS.")]
    public async Task<string> GetLandingPageById(
        [Description("The unique identifier (GUID) of the landing page, e.g. '3fa85f64-5717-4562-b3fc-2c963f66afa6'")] string id)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"{_tenantContext.TenantId}/api/landingpage/id/{Uri.EscapeDataString(id)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
