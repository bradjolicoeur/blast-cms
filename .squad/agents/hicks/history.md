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

### P0 API Key Header Bug Fix (2026-03-30)

**Bug:** `src/blastcms.McpServer/Program.cs` was sending `X-API-Key` as the HTTP header for the downstream CMS API key. The REST API (`src/blastcms.web/Attributes/ApiKeyAttribute.cs`, `ApiKeyFullAttribute.cs`, `ApiAuthorizationHandler.cs`) reads `ApiKey`. This meant **all MCP tool calls were silently unauthenticated** — every request hit the API without a valid key.

**Fix:** Changed `client.DefaultRequestHeaders.Add("X-API-Key", cmsApiKey)` → `client.DefaultRequestHeaders.Add("ApiKey", cmsApiKey)` in Program.cs.

**Doc fix:** Updated `McpServerUserGuide.md` line 334 to say `ApiKey` (not `X-API-Key`) and added key-type guidance: full-access key enables write tools; read-only key causes 401 on write tools.

**Test fix:** `ApiKeyHeaderTests.cs` (added by Ripley, untracked) was failing because the mock `HttpClient` in the test didn't simulate `DefaultRequestHeaders` setup from Program.cs. Fixed by adding `mockHttpClient.DefaultRequestHeaders.Add("ApiKey", "test-api-key")` in the test helper, matching what Program.cs does. All 45 tests now pass.

**Status:** ✅ COMPLETE. Fix committed (28e1ee1). Ready for merge.

### MCP Tool Expansion — P1 Complete (2026-03-30)

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

### AutoMapper 13 → 15 Upgrade (2026-03-30)

Upgraded AutoMapper from 13.0.1 → 15.1.1 to fix CVE-2026-32933 (Denial of Service via uncontrolled recursion).

**API Changes in v15:**
- `AddAutoMapper(typeof(Program))` no longer works — the method signature changed to require a configuration expression
- Correct usage: `builder.Services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()))`
- In v15, DI integration requires `ILoggerFactory` to be registered (AutoMapper now has logging support)

**Test Setup Changes:**
- Old (v13): `new MapperConfiguration(cfg => cfg.AddMaps(typeof(Program)))`
- New (v15): Use DI container to build mapper:
  ```csharp
  var services = new ServiceCollection();
  services.AddLogging(); // Required by AutoMapper 15
  services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));
  var provider = services.BuildServiceProvider();
  Mapper = provider.GetRequiredService<IMapper>();
  ```

**Verification:**
- Build: ✅ 0 errors, 5 pre-existing warnings (Semantic Kernel vulnerability, etc.)
- Tests: ✅ All 123 tests passing (12 FusionAuth + 45 MCP + 66 web tests)
- Mapping profiles: ✅ All 29 profiles auto-discovered and working (simple flat CreateMap patterns)

**Files Changed:**
- `src/blastcms.web/blastcms.web.csproj` — AutoMapper 13.0.1 → 15.1.1
- `src/blastcms.web.tests/blastcms.web.tests.csproj` — AutoMapper 13.0.1 → 15.1.1
- `src/blastcms.web/Program.cs` — Updated AddAutoMapper call
- `src/blastcms.web.tests/OneTimeStartup.cs` — Updated test mapper initialization

**Commit:** 7e7a823
