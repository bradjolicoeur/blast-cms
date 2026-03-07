# Implement Remote-Hosted MCP Server for Blast CMS API

## Description
To allow Large Language Models (LLMs) and AI agents to securely interact with the Blast CMS API without requiring consumers to install any local executables, we need to implement a remote-hosted Model Context Protocol (MCP) server.

This server will provide tools, prompts, and resources over HTTP using Server-Sent Events (SSE). It will be hosted alongside our existing Cloud Run infrastructure, allowing consumers to simply configure their AI clients to point to our public MCP endpoint.

## Architecture & Authentication
We will utilize the official [Model Context Protocol C# SDK (`ModelContextProtocol.AspNetCore`)](https://github.com/modelcontextprotocol/csharp-sdk). 

**Authentication Pipeline:**
Because this is an HTTP-based transport, the AI client will pass the consumer's existing Blast CMS API key in the standard HTTP request headers. The MCP server will integrate with our ASP.NET Core middleware to validate the key, extract the tenant identity, and execute tool calls on behalf of that specific consumer securely.

## Acceptance Criteria
- [ ] Install the `ModelContextProtocol.AspNetCore` NuGet package in the `blastcms.web` project (or a new dedicated API project).
- [ ] Configure the MCP Server and the HTTP transport layer (`.WithHttpTransport()`) in the application startup.
- [ ] Call `app.MapMcp()` to expose the `/mcp/sse` and `/mcp/messages` endpoints.
- [ ] Ensure the MCP HTTP endpoints sit behind our existing API Key authentication/authorization policies to lock down access.
- [ ] Create wrapper classes marked with `[McpServerToolType]` and expose relevant CMS capabilities using the `[McpServerTool]` attribute (e.g., `GetArticle()`, `ListArticles()`, `UpdateContent()`).
- [ ] Document the endpoints and verify they deploy correctly to our Google Cloud Run environments (`blast-cms-test` and `blast-cms`).
- [ ] **Create Consumer Integration Guides:** Publish documentation detailing how consumers can connect the following AI assistants to the remote Blast CMS MCP server:
  - **GitHub Copilot** (Configuration via VS Code settings)
  - **Claude** (Configuration via Claude Desktop `claude_desktop_config.json`)
  - **Gemini** (Configuration via IDE extensions / API setups)

---

## Technical Details & Implementation Drafts

### 1. ASP.NET Core Implementation Example
```csharp
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// Existing Authentication/Authorization Setup using API Keys
builder.Services.AddAuthentication() // ... your existing setup

// Configure MCP Server with HTTP Transport
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Map the MCP endpoints (SSE and message endpoints) and secure them
app.MapMcp().RequireAuthorization();

app.Run();
```

### 2. Drafts for Consumer Integration Guides (Acceptance Criteria Deliverable)

The documentation created for the Acceptance Criteria should look similar to these configurations:

#### GitHub Copilot (VS Code)
Users can add the remote MCP server to their workspace or user settings (`settings.json`).
```json
{
  "github.copilot.mcp.servers": {
    "blast-cms": {
      "type": "sse",
      "url": "https://api.blastcms.com/mcp/sse",
      "headers": {
        "X-API-Key": "YOUR_BLAST_API_KEY"
      }
    }
  }
}
```

#### Claude Desktop
Because Claude Desktop primarily supports Stdio commands, connecting to a remote SSE server requires using an official npx proxy bridge.
**`claude_desktop_config.json`:**
```json
{
  "mcpServers": {
    "blast-cms": {
      "command": "npx",
      "args": [
        "-y",
        "@modelcontextprotocol/sse-client-proxy",
        "https://api.blastcms.com/mcp/sse"
      ],
      "env": {
        "HTTP_HEADER_X_API_KEY": "YOUR_BLAST_API_KEY"
      }
    }
  }
}
```

#### Gemini
For Gemini clients or custom agents utilizing the Gemini API with MCP extensions, consumers will provide the SSE endpoint URL and the API Key header.
```json
{
  "mcpHub": {
    "servers": {
      "blast-cms": {
        "transport": "sse",
        "endpoint": "https://api.blastcms.com/mcp/sse",
        "authHeader": "X-API-Key",
        "authToken": "YOUR_BLAST_API_KEY"
      }
    }
  }
}
```

## References
- [MCP Official Documentation](https://modelcontextprotocol.io/)
- [MCP C# ASP.NET Core Documentation](https://modelcontextprotocol.github.io/csharp-sdk/concepts/getting-started.html)