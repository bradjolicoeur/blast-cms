using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace blastcms.McpServer.Tools;

/// <summary>
/// MCP tools for accessing Feed Articles in Blast CMS.
/// Feed Articles are imported or aggregated articles from external sources.
/// </summary>
[McpServerToolType]
public class FeedArticleTools
{
    private readonly IHttpClientFactory _httpClientFactory;

    public FeedArticleTools(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [McpServerTool]
    [Description("Retrieves a paginated list of feed articles from Blast CMS. Feed articles are imported from external sources and can be searched by keyword.")]
    public async Task<string> ListFeedArticles(
        [Description("Optional search term to filter feed articles by title, description, or keywords")] string? search = null,
        [Description("Page number to retrieve, starting at 1")] int page = 1,
        [Description("Number of feed articles per page (default is 10)")] int pageSize = 10)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var url = $"api/feedarticle/all?currentPage={page}&take={pageSize}&skip=0";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single feed article by its unique identifier (GUID) from Blast CMS.")]
    public async Task<string> GetFeedArticleById(
        [Description("The unique identifier (GUID) of the feed article, e.g. '3fa85f64-5717-4562-b3fc-2c963f66afa6'")] string id)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"api/feedarticle/{Uri.EscapeDataString(id)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
