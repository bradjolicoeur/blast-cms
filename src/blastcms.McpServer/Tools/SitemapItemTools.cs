using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace blastcms.McpServer.Tools;

/// <summary>
/// MCP tools for managing Sitemap Items in Blast CMS.
/// Sitemap Items define the URL structure for SEO and search engine discovery.
/// </summary>
[McpServerToolType]
public class SitemapItemTools
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SitemapItemTools(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [McpServerTool]
    [Description("Retrieves a paginated list of sitemap items from Blast CMS.")]
    public async Task<string> ListSitemapItems(
        [Description("Page number to retrieve, starting at 1")] int page = 1,
        [Description("Number of sitemap items per page (default is 10)")] int pageSize = 10)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var url = $"api/sitemapitem/all?currentPage={page}&take={pageSize}&skip=0";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
