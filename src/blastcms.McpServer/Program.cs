using blastcms.McpServer;

var builder = WebApplication.CreateBuilder(args);

// 1. Read configuration – all values are supplied via environment variables on Cloud Run
var baseUrl = builder.Configuration["BLAST_CMS_BASE_URL"] ?? "http://localhost:5000/";

// 2. Register IHttpContextAccessor for Bearer token passthrough
builder.Services.AddHttpContextAccessor();

// 3. Register Bearer passthrough handler — forwards client's Bearer token as ApiKey header
builder.Services.AddTransient<BearerPassthroughHandler>();

// 4. Configure downstream Blast CMS HTTP client
builder.Services.AddHttpClient(BlastCmsClientConstants.HttpClientName, client =>
{
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<BearerPassthroughHandler>();

// 5. Register tenant context as a singleton. The HTTP transport can execute tool
// invocations from the root MCP service provider, so TenantContext must read and
// write the current tenant from HttpContext rather than relying on DI scoping.
builder.Services.AddSingleton<TenantContext>();

// 6. Configure the MCP server with Streamable HTTP transport for remote deployment
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// 7. Cloud Run injects the PORT environment variable; respect it
var port = builder.Configuration["PORT"] ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");

var app = builder.Build();

// 8. Rewrite tenant-prefixed MCP paths before endpoint routing runs.
// Without an explicit UseRouting call here, minimal hosting inserts routing
// before user middleware, so /{tenant}/mcp gets rewritten too late and falls
// through as a 404.
app.UseMiddleware<TenantMiddleware>();
app.UseRouting();

// 9. Map all MCP protocol endpoints under /mcp
app.MapMcp("/mcp");

await app.RunAsync();
