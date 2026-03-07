using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
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
}
