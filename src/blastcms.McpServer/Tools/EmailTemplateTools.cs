using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace blastcms.McpServer.Tools;

/// <summary>
/// MCP tools for managing Email Templates in Blast CMS.
/// Email Templates are reusable templates for transactional and marketing emails.
/// </summary>
[McpServerToolType]
public class EmailTemplateTools
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EmailTemplateTools(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [McpServerTool]
    [Description("Retrieves a single email template by its unique identifier (GUID) from Blast CMS.")]
    public async Task<string> GetEmailTemplateById(
        [Description("The unique identifier (GUID) of the email template, e.g. '3fa85f64-5717-4562-b3fc-2c963f66afa6'")] string id)
    {
        var client = _httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName);
        var response = await client.GetAsync($"api/emailtemplate/id/{Uri.EscapeDataString(id)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
