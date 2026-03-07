using blastcms.McpServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// 1. Read the API key provided by the MCP client via environment variables
var apiKey = builder.Configuration["BLAST_CMS_API_KEY"] ?? string.Empty;
var baseUrl = builder.Configuration["BLAST_CMS_BASE_URL"] ?? "http://localhost:5000/";

// 2. Configure Downstream Blast CMS HTTP Client with the API key
builder.Services.AddHttpClient(BlastCmsClientConstants.HttpClientName, client =>
{
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
});

// 3. Configure the MCP Server with Stdio transport so agents like Claude Desktop can use it
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
