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

## Core Context

**MCP Tool Architecture:**
- Tools in `src/blastcms.McpServer/Tools/`. Each class uses `[McpServerToolType]` attribute; methods use `[McpServerTool]` and `[Description]`.
- Tool method names: use `[McpServerTool(Name = "snake_case_name")]` for snake_case per MCP conventions.
- Inject `IHttpClientFactory`; create client: `_httpClientFactory.CreateClient(BlastCmsClientConstants.HttpClientName)`.
- Read tools: `response.EnsureSuccessStatusCode()`, return `response.Content.ReadAsStringAsync()`.
- Write tools: use `PostAsJsonAsync`, return error details on non-success, otherwise response body.
- All tools now depend on scoped `TenantContext` for tenant-aware routing.

**REST API Endpoints:**
- **BlogArticle:** `POST api/blogarticle/` (upsert via omit/include `id`). Required: `title`, `slug`, `publishedDate`. Optional: `author`, `body`, `description`, `tags`.
- **ContentBlock:** `POST api/contentblock/` (same pattern). Required: `slug`. Optional: `title`, `body`, `groups`.
- Write endpoints protected by `[ApiKeyFull]` attribute (stricter than read).
- API uses single POST for both insert and update (Marten document store upsert pattern).

**Test Infrastructure:**
- In-process MCP test hosts must register all scoped dependencies with test values (e.g., `TenantContext` with `"test-tenant"`).
- Tool discovery can pass while invocation fails if scoped services are missing — always validate both.
- Pattern: each `CreateClientServerPair` includes `services.AddScoped(_ => new TenantContext { TenantId = "test-tenant" })`.

**Deployment:**
- CI test gate: `run-tests` job runs first in `.github/workflows/github-actions-push.yml`; both build jobs require `needs: run-tests`.
- MCP server uses bearer token passthrough: clients present `Authorization: Bearer <blast-cms-api-key>`, MCP forwards as `ApiKey` header downstream.
- Single env var needed: `BLAST_CMS_BASE_URL`. No secrets at MCP layer.
- Tenant routing: endpoint moved from `/mcp` to `/{tenant}/mcp`. Middleware extracts tenant, rewrites path, returns 400 for bare `/mcp`.

**Recent Work Summary (2026-03-30 to 2026-04-13):**
- Implemented P1 MCP tool expansion (3 → 12 entity types, 9 → 33 tools) with read-only coverage for LandingPage, ContentTag, ImageFile, Podcast, Event, EventVenue, UrlRedirect, SitemapItem, EmailTemplate
- Fixed P0 API key header bug (`X-API-Key` → `ApiKey`), added ApiKeyHeaderTests.cs with regression test
- Implemented tenant-aware routing: TenantContext scoped service, TenantMiddleware path rewriting, all tools prefixed with tenant ID
- Implemented bearer token passthrough authentication: BearerPassthroughHandler extracts Bearer token from client, forwards as ApiKey to CMS, eliminates separate MCP_API_KEY env var
- Added CI test gate: `run-tests` job blocks both build jobs before deploy
- AutoMapper upgrade 13.0.1 → 15.1.1 (DI config changes, ILoggerFactory registration required)
- Fixed test harness regression: registered TenantContext in all 7 MCP test files' CreateClientServerPair helpers
- All 134 tests passing; 22 failures classified as test harness breakage, not product regression

## Learnings

### 2026-04-13 — MCP Test Harness TenantContext Validation ✅ Complete

Validated that registering scoped `TenantContext` in all MCP test helpers resolves the 22 failing tests.

**Test Results:** Full solution 134/134 tests passing (MCP 45/45, Web 66/66, FusionAuth 12/12)

**Pattern Applied to 7 test files:**
Each `CreateClientServerPair` now includes:
```csharp
services.AddScoped(_ => new TenantContext { TenantId = "test-tenant" });
```

**Impact:** Test infrastructure now matches production scoped dependency requirements. Future additions of request-scoped services to MCP server must follow this pattern to maintain test/production parity.

**Orchestration Log:** `.squad/orchestration-log/2026-04-13T125604Z-hicks.md`
