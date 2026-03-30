using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
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

    /// <summary>
    /// Creates a new blog article via the Blast CMS REST API (POST api/blogarticle/).
    /// </summary>
    [McpServerTool(Name = "create_blog_article")]
    [Description("Creates a new blog article in Blast CMS. Title and slug are required.")]
    public async Task<string> CreateBlogArticle(
        [Description("Title of the blog article")] string title,
        [Description("URL slug for the blog article (e.g. 'my-first-post')")] string slug,
        [Description("Author of the blog article")] string? author = null,
        [Description("HTML or Markdown content of the article")] string? content = null,
        [Description("Short description or excerpt of the article")] string? description = null,
        [Description("Published date in ISO 8601 format (e.g. '2024-01-15T00:00:00Z')")] string? publishedDate = null,
        [Description("Comma-separated list of tags (e.g. 'technology,news')")] string? tags = null)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var command = new
        {
            title,
            slug,
            publishedDate,
            author,
            body = content,
            description,
            tags = tags?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? []
        };
        var response = await client.PostAsJsonAsync("api/blogarticle/", command);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error {(int)response.StatusCode} {response.ReasonPhrase}: {errorBody}", null, response.StatusCode);
        }
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Updates an existing blog article via the Blast CMS REST API (POST api/blogarticle/ with Id).
    /// </summary>
    [McpServerTool(Name = "update_blog_article")]
    [Description("Updates an existing blog article in Blast CMS by its GUID. Id, title, and slug are required.")]
    public async Task<string> UpdateBlogArticle(
        [Description("The unique identifier (GUID) of the blog article to update")] string id,
        [Description("Title of the blog article")] string title,
        [Description("URL slug for the blog article (e.g. 'my-first-post')")] string? slug = null,
        [Description("Author of the blog article")] string? author = null,
        [Description("HTML or Markdown content of the article")] string? content = null,
        [Description("Short description or excerpt of the article")] string? description = null,
        [Description("Published date in ISO 8601 format (e.g. '2024-01-15T00:00:00Z')")] string? publishedDate = null,
        [Description("Comma-separated list of tags (e.g. 'technology,news')")] string? tags = null)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var command = new
        {
            id,
            title,
            slug,
            publishedDate,
            author,
            body = content,
            description,
            tags = tags?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? []
        };
        var response = await client.PostAsJsonAsync("api/blogarticle/", command);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error {(int)response.StatusCode} {response.ReasonPhrase}: {errorBody}", null, response.StatusCode);
        }
        return await response.Content.ReadAsStringAsync();
    }
}
