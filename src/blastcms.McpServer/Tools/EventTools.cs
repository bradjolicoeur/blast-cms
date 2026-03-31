using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace blastcms.McpServer.Tools;

/// <summary>
/// MCP tools for managing Events in Blast CMS.
/// Events are time-based content items like conferences, webinars, or meetups.
/// </summary>
[McpServerToolType]
public class EventTools
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TenantContext _tenantContext;

    public EventTools(IHttpClientFactory httpClientFactory, TenantContext tenantContext)
    {
        _httpClientFactory = httpClientFactory;
        _tenantContext = tenantContext;
    }

    [McpServerTool]
    [Description("Retrieves a paginated list of all events from Blast CMS. Supports optional search and tag filtering.")]
    public async Task<string> ListEvents(
        [Description("Optional search term to filter events by title or description")] string? search = null,
        [Description("Optional tag to filter events by")] string? tag = null,
        [Description("Page number to retrieve, starting at 1")] int page = 1,
        [Description("Number of events per page (default is 10)")] int pageSize = 10)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var url = $"{_tenantContext.TenantId}/api/event/all?currentPage={page}&take={pageSize}&skip=0";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrWhiteSpace(tag))
            url += $"&tag={Uri.EscapeDataString(tag)}";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a paginated list of recent and upcoming events from Blast CMS. Returns events that have happened recently or will happen in the future.")]
    public async Task<string> ListRecentEvents(
        [Description("Number of days in the past to include recent events (default is 30)")] int days = 30,
        [Description("Optional search term to filter events by title or description")] string? search = null,
        [Description("Optional tag to filter events by")] string? tag = null,
        [Description("Page number to retrieve, starting at 1")] int page = 1,
        [Description("Number of events per page (default is 10)")] int pageSize = 10)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var url = $"{_tenantContext.TenantId}/api/event/recent?currentPage={page}&take={pageSize}&skip=0&days={days}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrWhiteSpace(tag))
            url += $"&tag={Uri.EscapeDataString(tag)}";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single event by its URL slug from Blast CMS.")]
    public async Task<string> GetEventBySlug(
        [Description("The URL slug of the event (e.g. 'annual-conference-2024')")] string slug)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"{_tenantContext.TenantId}/api/event/slug/{Uri.EscapeDataString(slug)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single event by its unique identifier (GUID) from Blast CMS.")]
    public async Task<string> GetEventById(
        [Description("The unique identifier (GUID) of the event, e.g. '3fa85f64-5717-4562-b3fc-2c963f66afa6'")] string id)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"{_tenantContext.TenantId}/api/event/{Uri.EscapeDataString(id)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
