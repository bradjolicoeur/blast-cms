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

// 5. Register scoped tenant context — populated per-request by TenantMiddleware
builder.Services.AddScoped<TenantContext>();

// 6. Configure the MCP server with Streamable HTTP transport for remote deployment
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// 7. Cloud Run injects the PORT environment variable; respect it
var port = builder.Configuration["PORT"] ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");

var app = builder.Build();

// 8. Extract tenant from /{tenant}/mcp path prefix; reject bare /mcp requests
app.UseMiddleware<TenantMiddleware>();

// 9. Map all MCP protocol endpoints under /mcp
app.MapMcp("/mcp");

await app.RunAsync();
