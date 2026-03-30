using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace blastcms.McpServer.Tools;

/// <summary>
/// MCP tools for managing Content Blocks in Blast CMS.
/// Content Blocks are reusable pieces of content that can be embedded on pages.
/// </summary>
[McpServerToolType]
public class ContentBlockTools
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ContentBlockTools(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [McpServerTool]
    [Description("Retrieves a paginated list of content blocks from Blast CMS. Content blocks are reusable pieces of content identified by a slug or group.")]
    public async Task<string> ListContentBlocks(
        [Description("Optional search term to filter content blocks by title or content")] string? search = null,
        [Description("Page number to retrieve, starting at 1")] int page = 1,
        [Description("Number of content blocks per page (default is 10)")] int pageSize = 10)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var url = $"api/contentblock/all?currentPage={page}&take={pageSize}&skip=0";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single content block by its URL slug from Blast CMS. Slugs are human-readable identifiers used to reference content blocks.")]
    public async Task<string> GetContentBlockBySlug(
        [Description("The URL slug of the content block (e.g. 'hero-banner', 'footer-text')")] string slug)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"api/contentblock/slug/{Uri.EscapeDataString(slug)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves all content blocks belonging to a specific content group from Blast CMS. Groups allow organizing related content blocks together.")]
    public async Task<string> GetContentBlocksByGroup(
        [Description("The name of the content group to retrieve content blocks for (e.g. 'homepage', 'sidebar')")] string group)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"api/contentblock/group/{Uri.EscapeDataString(group)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single content block by its unique identifier (GUID) from Blast CMS.")]
    public async Task<string> GetContentBlockById(
        [Description("The unique identifier (GUID) of the content block, e.g. '3fa85f64-5717-4562-b3fc-2c963f66afa6'")] string id)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"api/contentblock/id/{Uri.EscapeDataString(id)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Creates a new content block via the Blast CMS REST API (POST api/contentblock/).
    /// </summary>
    [McpServerTool(Name = "create_content_block")]
    [Description("Creates a new content block in Blast CMS. Slug is required.")]
    public async Task<string> CreateContentBlock(
        [Description("URL slug for the content block (e.g. 'hero-banner')")] string slug,
        [Description("Title of the content block")] string? title = null,
        [Description("HTML or Markdown content of the block")] string? content = null,
        [Description("Comma-separated list of content groups this block belongs to (e.g. 'homepage,sidebar')")] string? groups = null)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var command = new
        {
            slug,
            title,
            body = content,
            groups = groups?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? []
        };
        var response = await client.PostAsJsonAsync("api/contentblock/", command);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error {(int)response.StatusCode} {response.ReasonPhrase}: {errorBody}", null, response.StatusCode);
        }
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Updates an existing content block via the Blast CMS REST API (POST api/contentblock/ with Id).
    /// </summary>
    [McpServerTool(Name = "update_content_block")]
    [Description("Updates an existing content block in Blast CMS by its GUID.")]
    public async Task<string> UpdateContentBlock(
        [Description("The unique identifier (GUID) of the content block to update")] string id,
        [Description("URL slug for the content block (e.g. 'hero-banner')")] string? slug = null,
        [Description("Title of the content block")] string? title = null,
        [Description("HTML or Markdown content of the block")] string? content = null,
        [Description("Comma-separated list of content groups this block belongs to (e.g. 'homepage,sidebar')")] string? groups = null)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var command = new
        {
            id,
            slug,
            title,
            body = content,
            groups = groups?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? []
        };
        var response = await client.PostAsJsonAsync("api/contentblock/", command);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error {(int)response.StatusCode} {response.ReasonPhrase}: {errorBody}", null, response.StatusCode);
        }
        return await response.Content.ReadAsStringAsync();
    }
}
