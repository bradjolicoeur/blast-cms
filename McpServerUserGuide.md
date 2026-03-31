# Blast CMS MCP Server – User Guide

The Blast CMS MCP server exposes CMS content as [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) tools over HTTP, letting AI assistants such as **Claude**, **GitHub Copilot** (VS Code), and **Gemini CLI** read and query your CMS content through natural-language prompts.

The server runs as a **remote service** (deployed on GCP Cloud Run alongside your Blast CMS instance). Consumers connect to it with a URL — **no local installation is required**.

## Available Tools

| Tool name | Description |
|-----------|-------------|
| `list_blog_articles` | Paginated list of blog articles; supports `search` and `tag` filtering |
| `get_blog_article_by_slug` | Fetch a single blog article by URL slug |
| `get_blog_article_by_id` | Fetch a single blog article by GUID |
| `list_content_blocks` | Paginated list of reusable content blocks; supports `search` |
| `get_content_block_by_slug` | Fetch a content block by slug |
| `get_content_blocks_by_group` | Fetch all content blocks belonging to a group |
| `get_content_block_by_id` | Fetch a content block by GUID |
| `list_feed_articles` | Paginated list of aggregated feed articles; supports `search` |
| `get_feed_article_by_id` | Fetch a single feed article by GUID |

The MCP endpoint URL follows this pattern:

```
https://<your-mcp-server>.run.app/{tenant-id}/mcp
```

`{tenant-id}` is the tenant slug configured in the Blast CMS admin (e.g., `customer2`). Each tenant has its own slug; your administrator will tell you the correct value.

Your administrator will provide the exact URL and an API key (`MCP_API_KEY`) that you use as a Bearer token.

---

## Claude Desktop

Claude Desktop supports remote MCP servers configured with a URL and optional headers.

### Configuration file location

| OS | Path |
|----|------|
| macOS | `~/Library/Application Support/Claude/claude_desktop_config.json` |
| Windows | `%APPDATA%\Claude\claude_desktop_config.json` |

### Configuration

Open (or create) the configuration file and add an `mcpServers` entry:

```json
{
  "mcpServers": {
    "blast-cms": {
      "type": "http",
      "url": "https://<your-mcp-server>.run.app/{tenant-id}/mcp",
      "headers": {
        "Authorization": "Bearer <your-mcp-api-key>"
      }
    }
  }
}
```

Replace `<your-mcp-server>` with your Cloud Run service hostname, `{tenant-id}` with your tenant slug, and `<your-mcp-api-key>` with the token provided by your administrator.

> **Note**: If the server has no `MCP_API_KEY` configured (open access), omit the `headers` block entirely.

### Verifying the connection

1. Restart Claude Desktop.
2. Open a new conversation.
3. Click the **🔧 Tools** icon – `blast-cms` should appear in the list.
4. Ask: *"List the latest blog articles from Blast CMS"*

### Example prompts

```
List the five most recent blog articles.
```

```
Get the blog article with the slug "getting-started-with-blast-cms".
```

```
Find all content blocks in the "homepage" group.
```

```
Search feed articles for anything about "machine learning".
```

---

## VS Code Copilot (GitHub Copilot Agent Mode)

GitHub Copilot in VS Code supports remote MCP servers in **agent mode** (requires VS Code 1.99 or later and the GitHub Copilot Chat extension).

### Workspace configuration (recommended)

Create or edit `.vscode/mcp.json` in your repository root:

```json
{
  "servers": {
    "blast-cms": {
      "type": "http",
      "url": "https://<your-mcp-server>.run.app/{tenant-id}/mcp",
      "headers": {
        "Authorization": "Bearer <your-mcp-api-key>"
      }
    }
  }
}
```

> **Tip**: Store the API key as a VS Code secret or environment variable instead of committing it.  
> You can reference VS Code input variables: replace the value with `${input:mcpApiKey}` and add an `inputs` block at the top of `mcp.json`:
> ```json
> {
>   "inputs": [
>     {
>       "type": "promptString",
>       "id": "mcpApiKey",
>       "description": "Blast CMS MCP API Key",
>       "password": true
>     }
>   ],
>   "servers": {
>     "blast-cms": {
>       "type": "http",
>       "url": "https://<your-mcp-server>.run.app/{tenant-id}/mcp",
>       "headers": {
>         "Authorization": "Bearer ${input:mcpApiKey}"
>       }
>     }
>   }
> }
> ```

### User-level configuration (all workspaces)

Open VS Code settings (`Ctrl+,` / `Cmd+,`), search for **MCP**, and add the server entry under **GitHub Copilot › Mcp: Servers** in `settings.json`:

```json
{
  "github.copilot.chat.mcp.servers": {
    "blast-cms": {
      "type": "http",
      "url": "https://<your-mcp-server>.run.app/{tenant-id}/mcp",
      "headers": {
        "Authorization": "Bearer <your-mcp-api-key>"
      }
    }
  }
}
```

### Verifying the connection

1. Open the Copilot Chat panel(`Ctrl+Alt+I` / `Cmd+Alt+I`).
2. Switch to **Agent** mode using the mode selector at the bottom of the chat input.
3. Click the **🔧** (tools) icon – `blast-cms` tools should be listed.
4. Ask: *"List the latest blog articles from Blast CMS"*

### Example prompts

```
List the latest blog articles from Blast CMS.
```

```
Get the content block with slug "site-footer".
```

```
Find all feed articles tagged "technology".
```

```
Show me all content blocks in the "sidebar" group.
```

---

## GitHub Copilot CLI

[GitHub Copilot CLI](https://docs.github.com/en/copilot/copilot-cli/about-github-copilot-cli) integrates MCP servers for enhanced AI assistance in the terminal.

### Configuration file location

| Environment | Path |
|---|---|
| Per-repository | `.copilot/mcp.json` |
| Global | `~/.copilot/mcp.json` |

> **Tip**: Use per-repository configuration to version control the server settings with your project.

### Configuration

Create or edit `.copilot/mcp.json` in your repository root:

```json
{
  "mcpServers": {
    "blast-cms": {
      "type": "http",
      "url": "https://<your-mcp-server>.run.app/{tenant-id}/mcp",
      "headers": {
        "Authorization": "Bearer <your-mcp-api-key>"
      }
    }
  }
}
```

Replace `<your-mcp-server>` with your Cloud Run service hostname and `<your-mcp-api-key>` with the token provided by your administrator.

For local development with `docker-compose`, use:

```json
{
  "mcpServers": {
    "blast-cms": {
      "type": "http",
      "url": "http://localhost:8090/{tenant-id}/mcp",
      "headers": {
        "Authorization": "Bearer <your-mcp-api-key>"
      }
    }
  }
}
```

> **Note**: If the server has no `MCP_API_KEY` configured (open access), omit the `headers` block entirely.

### Verifying the connection

1. Reload the MCP configuration: `copilot mcp reload` (or restart your terminal).
2. List available MCP servers: `copilot mcp list`
3. Confirm `blast-cms` appears in the output.
4. Ask: *"Using the blast-cms MCP server, list the latest blog articles"*

### Example prompts

```
Using blast-cms, list the five most recent blog articles.
```

```
Get the blog article with the slug "getting-started-with-blast-cms" using blast-cms.
```

```
Search blast-cms feed articles for anything about "machine learning".
```

```
Find all content blocks in the "homepage" group using blast-cms.
```

---

## Gemini CLI

[Gemini CLI](https://github.com/google-gemini/gemini-cli) (version 0.1.7 or later) supports remote MCP servers through its settings file.

### Configuration file location

| OS | Path |
|----|------|
| macOS / Linux | `~/.gemini/settings.json` |
| Windows | `%USERPROFILE%\.gemini\settings.json` |

### Configuration

Open (or create) `~/.gemini/settings.json` and add an `mcpServers` block:

```json
{
  "mcpServers": {
    "blast-cms": {
      "httpUrl": "https://<your-mcp-server>.run.app/{tenant-id}/mcp",
      "headers": {
        "Authorization": "Bearer <your-mcp-api-key>"
      }
    }
  }
}
```

### Verifying the connection

Start Gemini CLI:

```bash
gemini
```

List available tools to confirm the server registered:

```
/mcp
```

You should see `blast-cms` and its nine tools listed.

### Example prompts

```
List the latest blog articles from Blast CMS.
```

```
Get the blog article with the slug "my-first-post".
```

```
Show all content blocks in the "homepage" group.
```

```
Search feed articles for "open source".
```

---

## Deploying to GCP Cloud Run

The MCP server is a standard ASP.NET Core web application and ships as a Docker container.

### Environment variables

Configure the following environment variables on your Cloud Run service:

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `BLAST_CMS_API_KEY` | **Yes** | *(none)* | API key for the Blast CMS API. Sent as `ApiKey` on every downstream request. Use a **full-access key** to enable write tools (`create_blog_article`, `create_content_block`, etc.); a **read-only key** works for list/get tools but write tools will return 401. |
| `BLAST_CMS_BASE_URL` | **Yes** | *(none)* | Base URL of your Blast CMS API (the web service URL on Cloud Run). Must end with `/`. |
| `MCP_API_KEY` | Recommended | *(empty – unauthenticated)* | Bearer token clients must supply in the `Authorization` header. If empty, the endpoint is unauthenticated. |
| `PORT` | No | `8080` | Listening port. Cloud Run sets this automatically; do not override it. |

### Building and pushing the container

```bash
# From the repository root
docker build -f src/Dockerfile.mcpserver -t gcr.io/<project>/blastcms-mcp:latest src/

docker push gcr.io/<project>/blastcms-mcp:latest
```

### Deploying to Cloud Run

```bash
gcloud run deploy blastcms-mcp \
  --image gcr.io/<project>/blastcms-mcp:latest \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated \
  --set-env-vars BLAST_CMS_API_KEY=<cms-api-key> \
  --set-env-vars BLAST_CMS_BASE_URL=https://<your-blast-cms>.run.app/ \
  --set-env-vars MCP_API_KEY=<mcp-api-key>
```

After deployment, the MCP endpoint URL is:

```
https://blastcms-mcp-<hash>-uc.a.run.app/{tenant-id}/mcp
```

Distribute this URL (with each team member's tenant ID substituted) and the `MCP_API_KEY` value to your team members.

### Running locally with Docker Compose

For local development you can run the MCP server alongside the other services:

```bash
# Add these variables to your .env file first:
# BLAST_CMS_API_KEY=your-cms-api-key
# BLAST_CMS_BASE_URL=http://host.docker.internal:5000/
# MCP_API_KEY=your-mcp-api-key

docker compose up blastcms-mcp
```

The MCP endpoint will be available at `http://localhost:8090/{tenant-id}/mcp`.

---

## Troubleshooting

### 401 Unauthorized from the MCP endpoint

- Confirm the `Authorization: Bearer <token>` header value matches the `MCP_API_KEY` configured on the server.
- If no key was configured, omit the `Authorization` header.

### Tools not appearing in the client

- Verify the URL is reachable from your machine: `curl https://<your-mcp-server>.run.app/{tenant-id}/mcp`
- Confirm the client type is set to `"http"` (VS Code, Claude) or `"httpUrl"` key (Gemini CLI), not `"stdio"`.

### 400 Bad Request from the MCP endpoint

- Verify the URL includes the tenant identifier: `/{tenant-id}/mcp`. The endpoint `https://<your-mcp-server>.run.app/mcp` (without a tenant prefix) returns 400 by design.

### API calls fail with 401 (CMS)

- Verify `BLAST_CMS_API_KEY` on the server matches a key configured in the Blast CMS admin UI.
- Ensure `BLAST_CMS_BASE_URL` points to the correct tenant URL.

