using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace blastcms.McpServer.Tools;

/// <summary>
/// MCP tools for managing Content Tags in Blast CMS.
/// Content Tags are used to categorize and organize content.
/// </summary>
[McpServerToolType]
public class ContentTagTools
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ContentTagTools(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [McpServerTool]
    [Description("Retrieves a paginated list of content tags from Blast CMS.")]
    public async Task<string> ListContentTags(
        [Description("Page number to retrieve, starting at 1")] int page = 1,
        [Description("Number of content tags per page (default is 10)")] int pageSize = 10)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var url = $"api/contenttag/all?currentPage={page}&take={pageSize}&skip=0";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
