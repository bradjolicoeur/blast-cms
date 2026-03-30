using blastcms.McpServer;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Read configuration – all values are supplied via environment variables on Cloud Run
var cmsApiKey = builder.Configuration["BLAST_CMS_API_KEY"] ?? string.Empty;
var baseUrl = builder.Configuration["BLAST_CMS_BASE_URL"] ?? "http://localhost:5000/";
var mcpApiKey = builder.Configuration["MCP_API_KEY"] ?? string.Empty;

// 2. Configure downstream Blast CMS HTTP client
builder.Services.AddHttpClient(BlastCmsClientConstants.HttpClientName, client =>
{
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("ApiKey", cmsApiKey);
});

// 3. Configure the MCP server with Streamable HTTP transport for remote deployment
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// 4. Cloud Run injects the PORT environment variable; respect it
var port = builder.Configuration["PORT"] ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");

var app = builder.Build();

// 5. Protect the /mcp endpoint with a Bearer token when MCP_API_KEY is configured
if (!string.IsNullOrEmpty(mcpApiKey))
{
    var expectedTokenBytes = Encoding.UTF8.GetBytes(mcpApiKey);

    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/mcp"))
        {
            var authorized = false;
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var header = authHeader.ToString();
                if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var providedTokenBytes = Encoding.UTF8.GetBytes(header["Bearer ".Length..]);
                    authorized = CryptographicOperations.FixedTimeEquals(providedTokenBytes, expectedTokenBytes);
                }
            }

            if (!authorized)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }
        await next(context);
    });
}

// 6. Map all MCP protocol endpoints under /mcp
app.MapMcp("/mcp");

await app.RunAsync();
