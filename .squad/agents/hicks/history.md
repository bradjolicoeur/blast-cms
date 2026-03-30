# Project Context

- **Owner:** Brad Jolicoeur
- **Project:** blast-cms — a headless content management system built on .NET with Blazor Server. Websites consume content via REST API. Admin interface is Blazor Server.
- **Stack:** .NET (C#), Blazor Server, REST API, FusionAuth for identity, Google Cloud Storage for assets, MCP server for AI integrations. Multi-project solution at `src/`.
- **Created:** 2026-03-30

## Key Projects (Backend Focus)

- `blastcms.handlers` — REST API request handlers / endpoints (primary ownership)
- `blastcms.ArticleScanService` — background service for article scanning
- `blastcms.ImageResizeService` — background service for image resizing
- `blastcms.FusionAuthService` — FusionAuth identity integration
- `blastcms.McpServer` — MCP server for AI tool integrations
- `blastcms.UserManagement` — user management layer

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### MCP Tool Patterns (2026-03-30)

- MCP tools live in `src/blastcms.McpServer/Tools/`. Each class is decorated with `[McpServerToolType]` and each method with `[McpServerTool]` and `[Description]`.
- Tool method names default to the C# method name; use `[McpServerTool(Name = "snake_case_name")]` to override to snake_case per MCP conventions.
- Inject `IHttpClientFactory` via constructor; create client with `_httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName)`.
- Read tools use `response.EnsureSuccessStatusCode()` and return `await response.Content.ReadAsStringAsync()`.
- Write tools use `PostAsJsonAsync` (from `System.Net.Http.Json`) and return `$"Error {statusCode} {reason}: {body}"` on non-success, otherwise the response body.

### REST API Endpoints Discovered

- **BlogArticle create/update:** `POST api/blogarticle/` — single endpoint; omit `id` to create, include `id` (GUID) to update. Required fields: `title`, `slug`, `publishedDate`. Optional: `author`, `body`, `description`, `tags` (array of strings).
- **ContentBlock create/update:** `POST api/contentblock/` — same pattern; omit `id` to create, include `id` to update. Required: `slug`. Optional: `title`, `body`, `groups` (array of strings).
- Write endpoints are protected by `[ApiKeyFull]` attribute (stricter key than read endpoints).

### Notable API Quirks

- The CMS uses a single POST endpoint for both insert and update (upsert via Marten document store). There are no separate PUT endpoints.
- `ContentBlock.Groups` is a `HashSet<string>` (not `Tags` like BlogArticle). MCP tools accept a comma-separated string and split it server-side before posting.
