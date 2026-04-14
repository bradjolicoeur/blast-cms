# Blast CMS

A fun Headless CMS that uses MartenDb and Blazor Server.

This project includes an **MCP (Model Context Protocol) server** that enables AI agents (Claude, GitHub Copilot, Gemini) to query CMS content through natural language. See [McpServerUserGuide.md](McpServerUserGuide.md) for setup and integration instructions.

## VS Code Copilot MCP setup

The Blast CMS MCP server uses **HTTP transport**, requires a tenant-aware endpoint in the form `/{tenant-id}/mcp`, and expects clients to send `Authorization: Bearer <your-blast-cms-api-key>`. The MCP server forwards that bearer token to Blast CMS as the `ApiKey` header, so there is **no separate MCP password or token** to configure in VS Code.

### Prerequisites

- VS Code 1.99 or later with the **GitHub Copilot Chat** extension
- Copilot Chat running in **Agent** mode
- A reachable MCP server URL:
  - **Remote:** `https://<your-mcp-host>/{tenant-id}/mcp`
  - **Local Docker Compose:** `http://localhost:8090/{tenant-id}/mcp`
- For local Docker Compose, the main Blast CMS app must already be reachable at `BLAST_CMS_BASE_URL` (the compose file defaults this to `http://host.docker.internal:5000/`)

### Recommended workspace config

Create `.vscode/mcp.json` in the repo root:

```json
{
  "servers": {
    "blast-cms": {
      "type": "http",
      "url": "https://<your-mcp-host>/{tenant-id}/mcp",
      "headers": {
        "Authorization": "Bearer <your-blast-cms-api-key>"
      }
    }
  }
}
```

- Replace the URL with `http://localhost:8090/{tenant-id}/mcp` when using the local `blastcms-mcp` container from `docker-compose.yml`
- Replace `{tenant-id}` with your tenant slug
- Use your existing **Blast CMS API key** as the bearer token

If you prefer user-level VS Code configuration, put the same server object under `github.copilot.chat.mcp.servers` in your `settings.json`.

### Verify it works

1. Reload VS Code after saving the MCP config
2. Open **Copilot Chat**
3. Switch to **Agent** mode
4. Open the **Tools** list and confirm `blast-cms` appears

For full MCP setup details for Copilot, Claude, Gemini CLI, Cloud Run, and local Docker usage, see [McpServerUserGuide.md](McpServerUserGuide.md).

## Secrets

You will need to add the following secrets for this project to work.  If you are using visual studio, I recommend using the manage secrets option instead of adding these to your `appsettings.json` or `launchsettings.json` file.

```json
{
  "TINIFY_API_KEY": "register on https://tinypng.com/developers to get our free key",
  "Kestrel:Certificates:Development:Password": "kestrel cert key for local development",
  "Auth0": {
    "Domain": "your domain",
    "ClientId": "your client id",
    "ClientSecret": "your client secret"
  }
}
```

You will also need to [create a google credentials file](https://cloud.google.com/dotnet/docs/setup) and set the path in the `launchsettings.json` file.

```json
"GoogleCredentialFile": "c:/data/temp/bradjolicoeur-web-f75a278433e9.json"
```

## External Service Dependencies

- [Auth0](https://auth0.com/)
  - Authentication
- [TinyPNG](https://tinypng.com/developers)
  - Optimize image sizes during upload
- [Google Cloud Storage](https://cloud.google.com/storage)
  - Storage of images
