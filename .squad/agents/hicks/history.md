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

### P1 MCP Read Tools Implementation ✅ Complete (2026-03-30)

Expanded MCP server coverage from 3 entity types to 12 entity types to support website-building use cases. All new tools follow the same pattern as existing tools (read-only, return raw JSON).

**New Tool Classes Created:**
1. **LandingPageTools** — 3 tools: `ListLandingPages`, `GetLandingPageBySlug`, `GetLandingPageById`
2. **ContentTagTools** — 1 tool: `ListContentTags`
3. **ImageFileTools** — 3 tools: `ListImageFiles`, `GetImageFileById`, `GetImageFileByTitle`
4. **PodcastTools** — 6 tools covering both Podcast and PodcastEpisode entities: `ListPodcasts`, `GetPodcastBySlug`, `GetPodcastById`, `ListPodcastEpisodes`, `GetPodcastEpisodeBySlug`, `GetPodcastEpisodeById`
5. **EventTools** — 4 tools: `ListEvents`, `ListRecentEvents`, `GetEventBySlug`, `GetEventById`
6. **EventVenueTools** — 3 tools: `ListEventVenues`, `GetEventVenueBySlug`, `GetEventVenueById`
7. **UrlRedirectTools** — 2 tools: `ListUrlRedirects`, `GetUrlRedirectByFrom`
8. **SitemapItemTools** — 1 tool: `ListSitemapItems`
9. **EmailTemplateTools** — 1 tool: `GetEmailTemplateById` (API only exposes get-by-id endpoint)

**API Endpoints Mapped:**
- All controllers in `src/blastcms.web/Api/` were surveyed
- Only read endpoints (GET) were implemented per P1 scope
- Followed exact route patterns from controllers: `/api/{entity}/all`, `/api/{entity}/slug/{slug}`, `/api/{entity}/id/{id}`, `/api/{entity}/{id}`
- Special cases: Events have both `all` and `recent` list endpoints; Podcast endpoints cover both parent Podcast and child PodcastEpisode entities

**Auto-registration via `WithToolsFromAssembly()`:**
- All tool classes are auto-discovered by the MCP SDK via `[McpServerToolType]` attribute
- No manual registration in `Program.cs` required
- Build succeeded with 0 errors, 0 warnings

**P1 Final Status:**
- 24 new read tools implemented across 9 tool classes
- Coverage: 3 → 12 entity types, 9 → 33 total tools
- All tools align with Bishop's test specifications (commit 37c0f8f)
- Decisions merged to `.squad/decisions/decisions.md`
