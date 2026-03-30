using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace blastcms.McpServer.Tools;

/// <summary>
/// MCP tools for managing Event Venues in Blast CMS.
/// Event Venues are physical or virtual locations where events take place.
/// </summary>
[McpServerToolType]
public class EventVenueTools
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EventVenueTools(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [McpServerTool]
    [Description("Retrieves a paginated list of event venues from Blast CMS. Supports optional search filtering.")]
    public async Task<string> ListEventVenues(
        [Description("Optional search term to filter venues by name or location")] string? search = null,
        [Description("Page number to retrieve, starting at 1")] int page = 1,
        [Description("Number of venues per page (default is 10)")] int pageSize = 10)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var url = $"api/eventvenue/all?currentPage={page}&take={pageSize}&skip=0";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single event venue by its URL slug from Blast CMS.")]
    public async Task<string> GetEventVenueBySlug(
        [Description("The URL slug of the event venue (e.g. 'conference-center-downtown')")] string slug)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"api/eventvenue/slug/{Uri.EscapeDataString(slug)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single event venue by its unique identifier (GUID) from Blast CMS.")]
    public async Task<string> GetEventVenueById(
        [Description("The unique identifier (GUID) of the event venue, e.g. '3fa85f64-5717-4562-b3fc-2c963f66afa6'")] string id)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"api/eventvenue/{Uri.EscapeDataString(id)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
