# MCP Tenant Test Harness

## When to use

Use this pattern for `blastcms.McpServer` in-process tests that build an MCP server with `AddMcpServer().WithStreamServerTransport(...)` and call tools through `McpClient`.

## Rule

If an MCP tool depends on scoped runtime context such as `TenantContext`, the test `ServiceCollection` must register that scoped dependency explicitly.

## Why

- `ListToolsAsync()` can pass without constructing the tool instance.
- `CallToolAsync()` activates the tool and will fail at runtime if scoped dependencies are missing.
- This failure mode can look like a product regression even when the implementation is fine.

## Minimal pattern

```csharp
var services = new ServiceCollection();
services.AddLogging(b => b.SetMinimumLevel(LogLevel.None));
services.AddSingleton(mockFactory.Object);
services.AddScoped(_ => new TenantContext { TenantId = "test-tenant" });

services
    .AddMcpServer()
    .WithStreamServerTransport(
        clientToServer.Reader.AsStream(),
        serverToClient.Writer.AsStream())
    .WithTools<BlogArticleTools>();
```

## In this repo

Applied in:
- `src/blastcms.McpServer.Tests/McpServerTests.cs`
- `src/blastcms.McpServer.Tests/ApiKeyHeaderTests.cs`
- `src/blastcms.McpServer.Tests/ContentTagToolTests.cs`
- `src/blastcms.McpServer.Tests/ImageFileToolTests.cs`
- `src/blastcms.McpServer.Tests/LandingPageToolTests.cs`
- `src/blastcms.McpServer.Tests/McpServerWriteToolTests.cs`
- `src/blastcms.McpServer.Tests/PodcastToolTests.cs`
