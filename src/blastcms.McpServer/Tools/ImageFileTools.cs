using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace blastcms.McpServer.Tools;

/// <summary>
/// MCP tools for managing Image Files in Blast CMS.
/// Image Files are media assets stored in the CMS.
/// </summary>
[McpServerToolType]
public class ImageFileTools
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TenantContext _tenantContext;

    public ImageFileTools(IHttpClientFactory httpClientFactory, TenantContext tenantContext)
    {
        _httpClientFactory = httpClientFactory;
        _tenantContext = tenantContext;
    }

    [McpServerTool]
    [Description("Retrieves a paginated list of image files from Blast CMS. Supports optional search and tag filtering.")]
    public async Task<string> ListImageFiles(
        [Description("Optional search term to filter images by title or description")] string? search = null,
        [Description("Optional tag to filter images by (e.g. 'hero', 'thumbnail')")] string? tag = null,
        [Description("Page number to retrieve, starting at 1")] int page = 1,
        [Description("Number of image files per page (default is 10)")] int pageSize = 10)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var url = $"{_tenantContext.TenantId}/api/imagefile/all?currentPage={page}&take={pageSize}&skip=0";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrWhiteSpace(tag))
            url += $"&tag={Uri.EscapeDataString(tag)}";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single image file by its unique identifier (GUID) from Blast CMS.")]
    public async Task<string> GetImageFileById(
        [Description("The unique identifier (GUID) of the image file, e.g. '3fa85f64-5717-4562-b3fc-2c963f66afa6'")] string id)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"{_tenantContext.TenantId}/api/imagefile/id/{Uri.EscapeDataString(id)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single image file by its title from Blast CMS.")]
    public async Task<string> GetImageFileByTitle(
        [Description("The title of the image file to search for")] string title)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"{_tenantContext.TenantId}/api/imagefile/title/{Uri.EscapeDataString(title)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
