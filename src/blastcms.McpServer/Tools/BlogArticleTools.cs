using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace blastcms.McpServer.Tools;

/// <summary>
/// MCP tools for managing Blog Articles in Blast CMS.
/// </summary>
[McpServerToolType]
public class BlogArticleTools
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BlogArticleTools(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [McpServerTool]
    [Description("Retrieves a paginated list of blog articles from Blast CMS. Supports optional search and tag filtering.")]
    public async Task<string> ListBlogArticles(
        [Description("Optional search term to filter articles by title or content")] string? search = null,
        [Description("Optional tag to filter articles by (e.g. 'technology', 'news')")] string? tag = null,
        [Description("Page number to retrieve, starting at 1")] int page = 1,
        [Description("Number of articles per page (default is 10)")] int pageSize = 10)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var url = $"api/blogarticle/all?currentPage={page}&take={pageSize}&skip=0";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrWhiteSpace(tag))
            url += $"&tag={Uri.EscapeDataString(tag)}";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single blog article by its URL slug from Blast CMS. The slug is the human-readable identifier used in the article URL.")]
    public async Task<string> GetBlogArticleBySlug(
        [Description("The URL slug of the blog article (e.g. 'my-first-post')")] string slug)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"api/blogarticle/slug/{Uri.EscapeDataString(slug)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single blog article by its unique identifier (GUID) from Blast CMS.")]
    public async Task<string> GetBlogArticleById(
        [Description("The unique identifier (GUID) of the blog article, e.g. '3fa85f64-5717-4562-b3fc-2c963f66afa6'")] string id)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"api/blogarticle/id/{Uri.EscapeDataString(id)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
