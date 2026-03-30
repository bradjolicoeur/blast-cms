# Project Context

- **Owner:** Brad Jolicoeur
- **Project:** blast-cms — a headless content management system built on .NET with Blazor Server. Websites consume content via REST API. Admin interface is Blazor Server.
- **Stack:** .NET (C#), Blazor Server, REST API, FusionAuth for identity, Google Cloud Storage for assets, MCP server for AI integrations. Multi-project solution at `src/`.
- **Created:** 2026-03-30

## Key Projects

- `blastcms.web` — Blazor Server admin UI
- `blastcms.handlers` — REST API request handlers / endpoints
- `blastcms.ArticleScanService` — background service for article scanning
- `blastcms.ImageResizeService` — background service for image resizing
- `blastcms.FusionAuthService` — FusionAuth identity integration
- `blastcms.McpServer` — MCP server for AI tool integrations
- `blastcms.UserManagement` — user management layer
- Test projects: `blastcms.web.tests`, `blastcms.McpServer.Tests`, `blastcms.FusionAuthService.Tests`

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### MCP Server Architecture (2026-03-30)
- **Transport:** Streamable HTTP via `ModelContextProtocol.AspNetCore` v1.1.0, mapped at `/mcp`. Hosted as ASP.NET Core web app on Cloud Run.
- **Auth:** Custom middleware — Bearer token from `MCP_API_KEY` env var, constant-time comparison. No auth when env var is empty.
- **Tool surface:** 9 read-only tools across 3 tool classes (BlogArticle: 3, ContentBlock: 4, FeedArticle: 2). All tools are GET-only proxies to the CMS REST API.
- **Coverage gap:** The CMS REST API has 48 endpoints across 13 controllers covering 15 entity types. The MCP server exposes only 3 of those entity types (~20% coverage). Missing: Podcast, PodcastEpisode, Event, EventVenue, LandingPage, ImageFile, ContentTag, UrlRedirect, SitemapItem, EmailTemplate, Tenant.
- **No write tools:** Zero create/update/delete MCP tools exist. All tools are read-only. This means agents cannot create or maintain content through MCP.
- **No MCP Resources:** Only tools are exposed; no MCP resource URIs are defined.
- **Tests:** 10 NUnit integration tests using in-process stream transport with mocked HTTP handlers. Cover tool registration and basic invocation for all 3 tool classes, plus 401 error paths. No tests for write operations (none exist).
- **User Guide:** Comprehensive `McpServerUserGuide.md` covers Claude Desktop, VS Code Copilot, and Gemini CLI registration with config examples, deployment instructions, and troubleshooting. Well done.
- **Dockerfile:** `src/Dockerfile.mcpserver` uses .NET 10 chiseled image. Docker Compose maps port 8090→8080.
