using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace blastcms.McpServer.Tools;

/// <summary>
/// MCP tools for managing Podcasts and Podcast Episodes in Blast CMS.
/// </summary>
[McpServerToolType]
public class PodcastTools
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TenantContext _tenantContext;

    public PodcastTools(IHttpClientFactory httpClientFactory, TenantContext tenantContext)
    {
        _httpClientFactory = httpClientFactory;
        _tenantContext = tenantContext;
    }

    [McpServerTool]
    [Description("Retrieves a paginated list of podcasts from Blast CMS. Supports optional search and tag filtering.")]
    public async Task<string> ListPodcasts(
        [Description("Optional search term to filter podcasts by title or description")] string? search = null,
        [Description("Optional tag to filter podcasts by")] string? tag = null,
        [Description("Page number to retrieve, starting at 1")] int page = 1,
        [Description("Number of podcasts per page (default is 10)")] int pageSize = 10)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var url = $"{_tenantContext.TenantId}/api/podcast/all?currentPage={page}&take={pageSize}&skip=0";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrWhiteSpace(tag))
            url += $"&tag={Uri.EscapeDataString(tag)}";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single podcast by its URL slug from Blast CMS.")]
    public async Task<string> GetPodcastBySlug(
        [Description("The URL slug of the podcast (e.g. 'tech-talks')")] string slug)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"{_tenantContext.TenantId}/api/podcast/slug/{Uri.EscapeDataString(slug)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single podcast by its unique identifier (GUID) from Blast CMS.")]
    public async Task<string> GetPodcastById(
        [Description("The unique identifier (GUID) of the podcast, e.g. '3fa85f64-5717-4562-b3fc-2c963f66afa6'")] string id)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"{_tenantContext.TenantId}/api/podcast/id/{Uri.EscapeDataString(id)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a paginated list of podcast episodes for a specific podcast from Blast CMS. Supports optional search and tag filtering.")]
    public async Task<string> ListPodcastEpisodes(
        [Description("The URL slug of the podcast to retrieve episodes for")] string podcastSlug,
        [Description("Optional search term to filter episodes by title or description")] string? search = null,
        [Description("Optional tag to filter episodes by")] string? tag = null,
        [Description("Page number to retrieve, starting at 1")] int page = 1,
        [Description("Number of episodes per page (default is 10)")] int pageSize = 10)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var url = $"{_tenantContext.TenantId}/api/podcastepisode/{Uri.EscapeDataString(podcastSlug)}/all?currentPage={page}&take={pageSize}&skip=0";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrWhiteSpace(tag))
            url += $"&tag={Uri.EscapeDataString(tag)}";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single podcast episode by its URL slug from Blast CMS.")]
    public async Task<string> GetPodcastEpisodeBySlug(
        [Description("The URL slug of the podcast episode (e.g. 'episode-1-introduction')")] string slug)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"{_tenantContext.TenantId}/api/podcastepisode/slug/{Uri.EscapeDataString(slug)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [McpServerTool]
    [Description("Retrieves a single podcast episode by its unique identifier (GUID) from Blast CMS.")]
    public async Task<string> GetPodcastEpisodeById(
        [Description("The unique identifier (GUID) of the podcast episode, e.g. '3fa85f64-5717-4562-b3fc-2c963f66afa6'")] string id)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"{_tenantContext.TenantId}/api/podcastepisode/id/{Uri.EscapeDataString(id)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
