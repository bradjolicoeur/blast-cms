# Blast CMS MCP Server – User Guide

The `blastcms.McpServer` project exposes Blast CMS content as [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) tools, letting AI assistants such as **Claude**, **GitHub Copilot** (VS Code), and **Gemini CLI** read and query your CMS content through natural-language prompts.

## Prerequisites

### 1. .NET 10 SDK

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download) and confirm it is available:

```bash
dotnet --version
```

### 2. A running Blast CMS instance

You need a Blast CMS API endpoint (local or remote) and a valid API key.  Generate an API key from the Blast CMS admin UI.

### 3. Build the MCP server

From the repository root, publish the server to a self-contained executable:

```bash
dotnet publish src/blastcms.McpServer/blastcms.McpServer.csproj \
  --configuration Release \
  --output ./publish/blastcms.McpServer
```

On Windows the output binary is `publish\blastcms.McpServer\blastcms.McpServer.exe`.  
On macOS/Linux it is `publish/blastcms.McpServer/blastcms.McpServer`.

> **Tip – development shortcut**: You can use `dotnet run --project src/blastcms.McpServer` instead of a published executable.  Replace the `command`/`args` examples below accordingly (see each section for details).

---

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

---

## Claude Desktop

Claude Desktop reads MCP server definitions from a JSON configuration file.

### Configuration file location

| OS | Path |
|----|------|
| macOS | `~/Library/Application Support/Claude/claude_desktop_config.json` |
| Windows | `%APPDATA%\Claude\claude_desktop_config.json` |

### Using the published executable

Open (or create) the configuration file and add a `mcpServers` entry:

```json
{
  "mcpServers": {
    "blast-cms": {
      "command": "/absolute/path/to/publish/blastcms.McpServer/blastcms.McpServer",
      "env": {
        "BLAST_CMS_API_KEY": "your-api-key-here",
        "BLAST_CMS_BASE_URL": "https://your-blast-cms-instance.com/"
      }
    }
  }
}
```

> **Windows example**
> ```json
> "command": "C:\\Projects\\blast-cms\\publish\\blastcms.McpServer\\blastcms.McpServer.exe"
> ```

### Using `dotnet run` (development)

```json
{
  "mcpServers": {
    "blast-cms": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/absolute/path/to/blast-cms/src/blastcms.McpServer"
      ],
      "env": {
        "BLAST_CMS_API_KEY": "your-api-key-here",
        "BLAST_CMS_BASE_URL": "https://your-blast-cms-instance.com/"
      }
    }
  }
}
```

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

GitHub Copilot in VS Code supports MCP servers in **agent mode** (requires VS Code 1.99 or later and the GitHub Copilot Chat extension).

### Workspace configuration (recommended)

Create or edit `.vscode/mcp.json` in your repository root:

```json
{
  "servers": {
    "blast-cms": {
      "type": "stdio",
      "command": "/absolute/path/to/publish/blastcms.McpServer/blastcms.McpServer",
      "env": {
        "BLAST_CMS_API_KEY": "your-api-key-here",
        "BLAST_CMS_BASE_URL": "https://your-blast-cms-instance.com/"
      }
    }
  }
}
```

> **Using `dotnet run` instead of a published binary:**
> ```json
> {
>   "servers": {
>     "blast-cms": {
>       "type": "stdio",
>       "command": "dotnet",
>       "args": [
>         "run",
>         "--project",
>         "${workspaceFolder}/src/blastcms.McpServer"
>       ],
>       "env": {
>         "BLAST_CMS_API_KEY": "your-api-key-here",
>         "BLAST_CMS_BASE_URL": "https://your-blast-cms-instance.com/"
>       }
>     }
>   }
> }
> ```
> The `${workspaceFolder}` variable expands to the repository root inside VS Code.

### User-level configuration (all workspaces)

Open VS Code settings (`Ctrl+,` / `Cmd+,`), search for **MCP**, and add the server entry under **GitHub Copilot › Mcp: Servers** in `settings.json`:

```json
{
  "github.copilot.chat.mcp.servers": {
    "blast-cms": {
      "type": "stdio",
      "command": "/absolute/path/to/publish/blastcms.McpServer/blastcms.McpServer",
      "env": {
        "BLAST_CMS_API_KEY": "your-api-key-here",
        "BLAST_CMS_BASE_URL": "https://your-blast-cms-instance.com/"
      }
    }
  }
}
```

### Verifying the connection

1. Open the Copilot Chat panel (`Ctrl+Alt+I` / `Cmd+Alt+I`).
2. Switch to **Agent** mode using the mode selector at the bottom of the chat input.
3. Click the **🔧** (tools) icon – `blast-cms` tools should be listed.
4. Ask: *"@agent List the latest blog articles from Blast CMS"*

### Example prompts

```
@agent List the latest blog articles from Blast CMS.
```

```
@agent Get the content block with slug "site-footer".
```

```
@agent Find all feed articles tagged "technology".
```

```
@agent Show me all content blocks in the "sidebar" group.
```

---

## Gemini CLI

[Gemini CLI](https://github.com/google-gemini/gemini-cli) (version 0.1.7 or later) supports MCP servers through its settings file.

### Configuration file location

| OS | Path |
|----|------|
| macOS / Linux | `~/.gemini/settings.json` |
| Windows | `%USERPROFILE%\.gemini\settings.json` |

### Using the published executable

Open (or create) `~/.gemini/settings.json` and add an `mcpServers` block:

```json
{
  "mcpServers": {
    "blast-cms": {
      "command": "/absolute/path/to/publish/blastcms.McpServer/blastcms.McpServer",
      "args": [],
      "env": {
        "BLAST_CMS_API_KEY": "your-api-key-here",
        "BLAST_CMS_BASE_URL": "https://your-blast-cms-instance.com/"
      }
    }
  }
}
```

### Using `dotnet run` (development)

```json
{
  "mcpServers": {
    "blast-cms": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/absolute/path/to/blast-cms/src/blastcms.McpServer"
      ],
      "env": {
        "BLAST_CMS_API_KEY": "your-api-key-here",
        "BLAST_CMS_BASE_URL": "https://your-blast-cms-instance.com/"
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

## Environment Variables Reference

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `BLAST_CMS_API_KEY` | **Yes** | *(empty)* | API key generated from the Blast CMS admin UI. Passed as `X-API-Key` on every request. |
| `BLAST_CMS_BASE_URL` | No | `http://localhost:5000/` | Base URL of your Blast CMS API. Must end with `/`. |

---

## Troubleshooting

### Tools do not appear in the client

- Confirm the `command` path is correct and the executable has the execute permission (`chmod +x` on macOS/Linux).
- Test the server directly in a terminal:
  ```bash
  BLAST_CMS_API_KEY=your-key BLAST_CMS_BASE_URL=https://your-instance.com/ \
    ./publish/blastcms.McpServer/blastcms.McpServer
  ```
  The process should start without errors and wait for input.

### API calls fail with 401 Unauthorized

- Verify that `BLAST_CMS_API_KEY` matches a key configured in the Blast CMS admin UI for the target tenant.
- Ensure `BLAST_CMS_BASE_URL` points to the correct tenant path (e.g. `https://your-instance.com/my-tenant/`).

### `dotnet run` is slow to start

Use `dotnet publish` to produce a native AOT or self-contained executable for production use.  The published binary starts significantly faster than `dotnet run`.
