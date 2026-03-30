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

### MCP Server Sprint PR Review (2026-03-30)
- **Verdict:** APPROVED. Branch `copilot/implement-mcp-server-blast-cms-api` ready to PR.
- **Scope reviewed:** 37 tools across 12 tool classes (was 9/3). 4 write tools (create/update for BlogArticle and ContentBlock). 24 new read tools covering LandingPage, ContentTag, ImageFile, Podcast, PodcastEpisode, Event, EventVenue, UrlRedirect, SitemapItem, EmailTemplate.
- **Tests:** 44 integration tests, all passing. Write tools cover success/401/404/400 paths.
- **Endpoint verification:** All 37 MCP tool endpoints verified against REST API controllers — zero mismatches.
- **Non-blocking notes:** (1) User guide Available Tools table is stale (lists 9, should list 37). (2) Write test uses `group` param name instead of `groups` for content block tools. (3) `CreateClientServerPair` duplicated across 5 test files — candidate for shared helper.
- **Copilot CLI docs:** New section added to McpServerUserGuide.md with correct config path/format. README cross-reference added.

### Security Audit — API Key Handling (2026-03-30)
- **Requested by:** Brad Jolicoeur — does the MCP server correctly follow the REST API's dual-key (read-only vs. full-access) design?
- **REST API design:** Two-tier API key model enforced via action filter attributes:
  - `[ApiKey]` (class-level on all 12 API controllers): Accepts any valid key (read-only or full-access). All GET endpoints use this.
  - `[ApiKeyFull]` (method-level on write endpoints): Rejects read-only keys with 401 "Api Key is readonly". Applied to all POST endpoints (create/update) across all controllers.
  - `ApiAuthorizationHandler` looks up key hash in DB, returns `Model(Valid, ro)`. The `SecureValue.Readonly` field stores the key type.
  - Header name: `ApiKey` (not `X-API-Key`).
  - Admin UI at `/apikeys` creates "Read only" or "Full Access" keys.
- **MCP server design:** Single `BLAST_CMS_API_KEY` env var, one `HttpClient`, one key for all operations. No read/write key distinction in the MCP layer itself. Relies on REST API's `[ApiKeyFull]` attribute to enforce write restrictions.
- **BUG FOUND — Wrong header name:** `Program.cs:16` sends header `X-API-Key` but REST API attributes read header `ApiKey`. All downstream API calls would fail with 401 in production. Tests don't catch this because they mock the HttpClient.
- **Doc gap:** `McpServerUserGuide.md` says nothing about which key type (read-only vs. full-access) to configure as `BLAST_CMS_API_KEY`. Users need guidance.
- **Decision written:** `.squad/decisions/inbox/ripley-api-key-security.md` — P0 bug fix + P1 doc update recommended.

### API Key Header Bug — FIXED (2026-03-30)
- **Status:** ✅ FIXED by Hicks (commit 28e1ee1)
- **Resolution:** Header name changed from `X-API-Key` to `ApiKey` in `Program.cs:16`. Documentation updated with key-type guidance. Bishop's regression test (commit 6b3378e) prevents reintroduction.
- **Test results:** All 45 tests passing.
- **Security review:** ✅ APPROVED. Dual-key design is sound; no architectural changes needed.
