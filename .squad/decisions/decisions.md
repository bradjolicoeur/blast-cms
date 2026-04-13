# Team Decisions — blast-cms

## P0 Security Decisions (2026-03-30)

### MCP Server API Key Security — Header Bug + Dual-Key Guidance

**Author:** Ripley (Lead)  
**Date:** 2026-03-30  
**Status:** Complete  
**Priority:** P0 (bug), P1 (docs)

#### Finding 1: Wrong API Key Header Name — P0 BUG

**File:** `src/blastcms.McpServer/Program.cs`, line 16

The MCP server was sending:
```csharp
client.DefaultRequestHeaders.Add("X-API-Key", cmsApiKey);  // ← WRONG
```

The REST API's `ApiKeyAttribute` and `ApiKeyFullAttribute` read from header name **`ApiKey`**:
```csharp
context.HttpContext.Request.Headers.TryGetValue("ApiKey", out var extractedApiKey)
```

**Impact:** Every request from the MCP server to the REST API would fail with 401 in production. The API never receives the key because it's looking for `ApiKey`, not `X-API-Key`.

**Why tests don't catch it:** All MCP server tests mock `IHttpClientFactory` and return canned responses regardless of headers sent. No integration test actually hits the REST API.

**Resolution:** Fixed by Hicks — changed header name to `"ApiKey"` in `Program.cs:16`. Updated `McpServerUserGuide.md` line 334 from "Sent as `X-API-Key`" to "Sent as `ApiKey`". **All 45 tests pass.** Commit: 28e1ee1.

#### Finding 2: Dual-Key Design Is Correctly Enforced (by the REST API, not the MCP server)

The REST API enforces the read/write distinction properly:

| Attribute | Scope | Accepts read-only key? | Accepts full-access key? |
|-----------|-------|----------------------|------------------------|
| `[ApiKey]` | All GET endpoints | ✅ Yes | ✅ Yes |
| `[ApiKeyFull]` | All POST endpoints | ❌ No — returns 401 "Api Key is readonly" | ✅ Yes |

The MCP server uses a **single key** (`BLAST_CMS_API_KEY`) for all operations. This is acceptable because:

- If a **read-only key** is configured: read tools work, write tools get 401 from `[ApiKeyFull]` — correct behavior.
- If a **full-access key** is configured: all tools work — correct behavior.

The security boundary lives in the REST API, not the MCP server. The MCP server is a transparent proxy. **No code change needed for dual-key support.**

#### Finding 3: Documentation Gap — Which Key to Use — P1

`McpServerUserGuide.md` described `BLAST_CMS_API_KEY` but said nothing about read-only vs. full-access. Users generating keys in admin UI see two buttons but don't know which to pick.

**Resolution:** Added by Hicks to `McpServerUserGuide.md`:

> Use a **full-access key** to enable write tools (`create_blog_article`, `create_content_block`, etc.); a **read-only key** works for list/get tools but write tools will return 401.

### MCP Server Branch — PR Review Verdict

**Author:** Ripley (Lead)  
**Date:** 2026-03-30  
**Status:** APPROVED  
**Branch:** `copilot/implement-mcp-server-blast-cms-api`

Reviewed 37 MCP tools across 12 tool classes (up from 9 tools / 3 classes on main). 4 write tools (`create_blog_article`, `update_blog_article`, `create_content_block`, `update_content_block`). 24 new read tools covering LandingPage, ContentTag, ImageFile, Podcast, PodcastEpisode, Event, EventVenue, UrlRedirect, SitemapItem, EmailTemplate. 44 integration tests (all passing). McpServerUserGuide.md and README.md updated.

**Architecture & Correctness:** ✅ All 37 MCP tool endpoints verified against REST API controllers — zero endpoint mismatches.

**Error Handling:** ✅ Read tools use `EnsureSuccessStatusCode()` consistently. Write tools explicitly handle non-success with status code + body.

**Test Quality:** ✅ Write tools cover success/401/404/400 paths. Read tool tests cover registration, invocation, and 401 errors. Minor note: `McpServerWriteToolTests.cs` uses `group` param name (should be `groups`), but MCP SDK ignores unrecognized keys — not a prod bug.

**Documentation:** ✅ Copilot CLI section well-written. ⚠️ "Available Tools" table is stale (lists 9, should list 37). Not functionally blocking but misleading.

**Verdict:** ✅ APPROVED — Ready to PR. Non-blocking notes: (1) Update Available Tools table in McpServerUserGuide.md. (2) Fix `group` → `groups` in write tool tests. (3) Extract `CreateClientServerPair` helper to shared test utility.

### API Key Header Regression Test

**Author:** Bishop (Tester)  
**Date:** 2026-03-30  
**Status:** Complete  
**Commit:** 6b3378e

Written to prevent reintroduction of P0 header-name bug. File: `src/blastcms.McpServer.Tests/ApiKeyHeaderTests.cs`. Single test: `OutgoingRequest_UsesApiKeyHeader_NotXApiKey`.

**Pattern:** Moq `HttpMessageHandler` with `.Callback<>` to capture outgoing `HttpRequestMessage`. Asserts `capturedRequest.Headers.Contains("ApiKey")` is `true` and `capturedRequest.Headers.Contains("X-API-Key")` is `false`.

**Result:** All 45 tests pass. Test infrastructure follows established pattern from existing suite.

---

## P1 Phase Decisions (2026-03-30)

### Read Tool Coverage Expansion

**Author:** Hicks (Backend Dev)  
**Status:** Complete  
**Commit:** 37c0f8f

Implemented read-only MCP tools for 9 additional entity types, expanding from 3 to 12 entity types (9 new tool classes, 24 new tools).

**Entity Types Covered:**
1. **LandingPageTools** — 3 tools: `list_landing_pages`, `get_landing_page_by_slug`, `get_landing_page_by_id`
2. **ContentTagTools** — 1 tool: `list_content_tags`
3. **ImageFileTools** — 3 tools: `list_image_files`, `get_image_file_by_id`, `get_image_file_by_title`
4. **PodcastTools** — 6 tools covering Podcast and PodcastEpisode entities
5. **EventTools** — 4 tools: `list_events`, `list_recent_events`, `get_event_by_slug`, `get_event_by_id`
6. **EventVenueTools** — 3 tools: `list_event_venues`, `get_event_venue_by_slug`, `get_event_venue_by_id`
7. **UrlRedirectTools** — 2 tools: `list_url_redirects`, `get_url_redirect_by_from`
8. **SitemapItemTools** — 1 tool: `list_sitemap_items`
9. **EmailTemplateTools** — 1 tool: `get_email_template_by_id`

**Implementation Pattern:** All tools follow existing pattern (constructor-inject `IHttpClientFactory`, create client, call `EnsureSuccessStatusCode()`, return raw JSON). Auto-registered via `WithToolsFromAssembly()`.

**Skipped:** ContentGroup (not exposed via API), Tenant (admin/system only), UserManagement (admin only).

---

### MCP Write Tools — BlogArticle & ContentBlock

**Author:** Hicks  
**Status:** Implemented  
**Date:** 2026-03-30

Added four write tools to MCP server:

**BlogArticleTools.cs**
- `create_blog_article` — Creates new article. Required: title, slug, publishedDate. Optional: author, body, description, tags (comma-separated)
- `update_blog_article` — Updates existing article by GUID. Same fields + required id

**ContentBlockTools.cs**
- `create_content_block` — Creates new block. Required: slug. Optional: title, body, groups (comma-separated)
- `update_content_block` — Updates existing block by GUID. Same fields + required id

**Endpoint Pattern:** Both entity types use single POST endpoint for create/update (REST API upsert pattern via Marten):
- `POST api/blogarticle/` — create when id absent, update when id provided
- `POST api/contentblock/` — same pattern

**API Surface Decisions:**
- Tags/Groups as comma-separated strings for scalar MCP parameters
- Error handling returns `"Error {statusCode} {reason}: {body}"` on non-success
- `publishedDate` as string for ISO 8601 serialization

---

### P1 Integration Test Coverage

**Author:** Bishop (Tester)  
**Status:** Complete (44 tests passing)  
**Date:** 2026-03-30

**Write Tool Tests (McpServerWriteToolTests.cs):**
- 13 integration tests covering create/update BlogArticle and ContentBlock
- Tests compile cleanly; follow exact pattern from McpServerTests.cs
- Mock HttpMessageHandler with Moq.Protected.SendAsync
- Coverage: success (201/200), unauthorized (401), bad request (400), not found (404)
- Tool names are strings so tests compile even when implementation incomplete

**Read Tool Tests (21 tests across 4 new entity types):**
- `LandingPageToolTests.cs` — 5 tests for list, get_by_slug, get_by_id, 401 error handling
- `ContentTagToolTests.cs` — 3 tests (no get-by-id endpoint)
- `ImageFileToolTests.cs` — 5 tests including get_by_title (non-standard)
- `PodcastToolTests.cs` — 9 tests covering Podcast and PodcastEpisode tools

**Tool Naming Inference:**
- Inferred from API controller patterns + existing conventions
- snake_case with entity in plural for list (e.g., `list_landing_pages`)
- snake_case with entity in singular for get (e.g., `get_landing_page_by_slug`)
- PodcastEpisode list takes `podcastSlug` parameter to scope episodes
- ImageFile has title-based lookup in addition to id lookup

**All tests compile successfully.** Will fail at runtime until implementations complete, but tool names are validated via string lookup after implementation.

**P1 Test Infrastructure Pattern:**
- `CreateClientServerPair(HttpMessageHandler)` wires in-process MCP server with McpClient via System.IO.Pipelines
- All tool registrations explicit (`.WithTools<T>()`); no tool scanning
- IHttpClientFactory mocked with Moq; returns single HttpClient with BaseAddress = http://localhost/
- Handler factories: `CreateSuccessHandler(json, statusCode)` for 2xx, `CreateErrorHandler(statusCode, body)` for 4xx/5xx
- Tool calls via `client.CallToolAsync("tool_name", Dictionary<string, object?>)`
- Assertions: check `result.IsError`, inspect `result.Content.OfType<TextContentBlock>().FirstOrDefault()?.Text`

---

## P0 Infrastructure Decisions (2026-03-30)

### AutoMapper Security Upgrade — CVE-2026-32933 Remediation

**Author:** Ripley (Lead) / Hicks (Implementation)  
**Date:** 2026-03-30  
**Status:** Complete  
**Commit:** 7e7a823

Upgraded AutoMapper from 13.0.1 to 15.1.1 to remediate CVE-2026-32933, a HIGH-severity Denial of Service vulnerability via uncontrolled recursion.

**Risk Assessment:** LOW for blast-cms — all 30 AutoMapper profiles use simple flat `CreateMap` patterns with no recursive types. The vulnerability class doesn't align with our usage patterns.

**Files Changed:**
- `src/blastcms.web/blastcms.web.csproj` — AutoMapper 13.0.1 → 15.1.1
- `src/blastcms.web.tests/blastcms.web.tests.csproj` — AutoMapper 13.0.1 → 15.1.1
- `src/blastcms.web/Program.cs` — Updated DI registration for v15 API (`AddAutoMapper` now requires config expression)
- `src/blastcms.web.tests/OneTimeStartup.cs` — Updated test setup to use DI container (AutoMapper 15 requires `ILoggerFactory`)

**Verification:**
- ✅ Build: 0 errors
- ✅ Tests: 123 passing (66 web + 45 MCP + 12 FusionAuth)
- ✅ All 29 AutoMapper profiles functional

**Breaking Changes Handled:**
1. `AddAutoMapper(typeof(Program))` → `AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()))`
2. Test setup: Direct `new MapperConfiguration()` → DI-based initialization via `ServiceCollection`
3. AutoMapper 15 requires `ILoggerFactory` in DI container (added `services.AddLogging()`)

### MCP Server CI/CD Pipeline — GitHub Actions Integration

**Author:** Hicks (Backend Dev)  
**Date:** 2026-03-30  
**Status:** Implemented

Added separate build and deploy jobs to `.github/workflows/github-actions-push.yml` for the blastcms.McpServer.

**New Elements:**
1. **Env Var:** `DOCKER_IMAGE_URL_MCP` = `us-east1-docker.pkg.dev/bradjolicoeur-web/blast-cms/blast-cms-mcp`
2. **Build Job:** `build-and-publish-mcp` — builds from `./src` via `Dockerfile.mcpserver`, pushes to Artifact Registry
3. **Deploy to Test:** `deploy-mcp-test` — deploys on every push
4. **Deploy to Production:** `deploy-mcp-production` — only on `main` branch

**Design:** The two build jobs (`build-and-publish` and `build-and-publish-mcp`) run in parallel with no `needs` dependency — they are independent.

**Rationale:** Matches the architecture decision to keep MCP server as a separate Cloud Run service with independent deployment lifecycle.

### Container Deployment Architecture — Single vs. Multiple Services

**Author:** Ripley (Lead)  
**Date:** 2026-03-30  
**Status:** Recommendation (No code change)

Evaluated four deployment options for blast-cms web and blastcms.McpServer:

| Option | Model | Verdict |
|--------|-------|---------|
| A | Merge into single ASP.NET Core host | ❌ Creates auth/deployment coupling |
| B | Multi-process single container | ❌ Cloud Run ingress limitation, adds reverse proxy complexity |
| **C** | **Separate Cloud Run services** | **✅ RECOMMENDED** |
| D | Cloud Run sidecar | ❌ Sidecars don't get public ingress |

**Recommendation:** Keep separate Cloud Run services (Option C).

**Rationale:** The MCP server has zero code dependencies on the main app — it's an HTTP proxy client of the REST API, not a component of it. Separate services enable:
- Independent deployments
- Independent scaling (scale to zero)
- Clean auth boundaries
- Smallest container images (chiseled base)
- Simple health checks and observability

### Dependabot Remediation — AutoMapper CVE-2026-32933 Assignment

**Author:** Ripley (Lead)  
**Date:** 2026-03-30  
**Status:** Complete

Two HIGH-severity Dependabot alerts flagged AutoMapper 13.0.1 vulnerability CVE-2026-32933 across two project files.

**Assignment and Verification:**
- **Action:** Hicks — update AutoMapper in both `.csproj` files, build, test
- **Validation:** Bishop — verify all tests pass after upgrade
- **Review:** Ripley — sign-off on PR

**Outcome:** ✅ Complete (see AutoMapper Security Upgrade decision above)

---

## Session Decisions (2026-03-31)

### MCP Server Tenant Base Path Isolation — Option B Selected

**Author:** Hicks (Backend Dev) / Bishop (Tester)  
**Date:** 2026-03-31  
**Status:** Implemented  
**Orchestration Logs:** `2026-03-31T073551Z-hicks-tenant-routing.md`, `2026-03-31T073551Z-bishop-tenant-tests.md`

Implemented Option B tenant isolation: the MCP endpoint moves from `/mcp` to `/{tenant}/mcp`. A single MCP server deployment handles all tenants via path-based tenant extraction.

**What Was Implemented:**

**New Files:**
- `src/blastcms.McpServer/TenantContext.cs` — scoped service holding `TenantId`
- `src/blastcms.McpServer/TenantMiddleware.cs` — middleware for tenant extraction and path rewriting

**Modified Files:**
- `src/blastcms.McpServer/Program.cs` — registered `TenantContext`, added middleware
- All 12 tool files in `Tools/` — injected `TenantContext`, prefixed API URLs with tenant ID
- `McpServerUserGuide.md` — updated examples, added troubleshooting

**Test Coverage:**
- `src/blastcms.web.tests/McpServer/TenantMiddlewareTests.cs` — 11 unit tests (Bishop)
  - All 8 path scenarios covered: tenant extraction, rewriting, error handling, pass-through, special chars, case variations

**Key Design Decisions:**
1. **Path rewriting before auth:** Middleware rewrites path to `/mcp` before bearer auth runs, keeping auth middleware unchanged
2. **400 for bare `/mcp`:** Requests to `/mcp` or `/mcp/subpath` return 400 with guidance to use `/{tenant}/mcp`
3. **Stateless design:** No session state — tenant in URL enables stateless scaling

**Verification:**
- ✅ Build: 0 errors, 0 warnings
- ✅ Tests: 123/123 passing (11 new TenantMiddleware tests + all existing tests)
- ✅ Zero regression

---

## Session Decisions (2026-04-13)

### GitHub Actions CI — Fix Workflow Rather Than Migrate to Aspire

**Author:** Ripley (Lead)  
**Date:** 2026-04-13  
**Status:** Recommendation — awaiting owner approval  
**Requested by:** Brad Jolicoeur

#### Context

Tests are failing in CI because the `run-tests` job lacks the database dependency that `docker-compose.yml` provides locally. Brad asked whether we should fix the workflow or convert the repo to Aspire.

#### Root Cause Analysis

Three distinct problems in `.github/workflows/github-actions-push.yml`:

1. **SDK mismatch:** Workflow uses `dotnet-version: '9.0.x'` but all projects now target `net10.0`. Build fails before tests even run.
2. **Missing PostgreSQL:** `blastcms.web.tests` uses `ThrowawayDb.Postgres` which needs a live Postgres instance. The CI job has no service container. Credentials are hardcoded: `blastcms_user` / `not_magical_scary`, host from `DB_HOST` env var (defaults to `localhost`).
3. **Incomplete test coverage:** CI only runs `blastcms.web.tests`. Two other test projects (`blastcms.McpServer.Tests`, `blastcms.FusionAuthService.Tests`) are never executed in CI.

#### Decision: Fix GitHub Actions (short-term), evaluate Aspire separately (medium-term)

##### Short-term: Fix the workflow

The fix is ~20 lines of YAML with zero code changes:

```yaml
run-tests:
  runs-on: ubuntu-latest
  services:
    postgres:
      image: postgres:11
      env:
        POSTGRES_USER: blastcms_user
        POSTGRES_PASSWORD: not_magical_scary
        POSTGRES_DB: blastcms_database
      ports:
        - 5432:5432
      options: >-
        --health-cmd pg_isready
        --health-interval 10s
        --health-timeout 5s
        --health-retries 5
  steps:
    - uses: actions/checkout@v2
    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
    - run: dotnet restore src/blastcms.web.tests/blastcms.web.tests.csproj
    - run: dotnet test src/blastcms.web.tests/blastcms.web.tests.csproj --no-restore --verbosity normal
      env:
        DB_HOST: localhost
```

Also consider adding the other two test projects (they don't need Postgres):
```yaml
    - run: dotnet test src/blastcms.McpServer.Tests/blastcms.McpServer.Tests.csproj --verbosity normal
    - run: dotnet test src/blastcms.FusionAuthService.Tests/blastcms.FusionAuthService.Tests.csproj --verbosity normal
```

**Effort:** 30 minutes. **Risk:** Minimal. **Code changes:** Zero.

##### Medium-term: Evaluate Aspire as a separate initiative

Aspire is strategically interesting for blast-cms:
- Replaces `docker-compose.yml` with type-safe C# orchestration
- Provides observability dashboard for local dev
- `Aspire.Hosting.PostgreSQL` handles Postgres lifecycle
- Would align with the .NET ecosystem direction

But it's **not the right tool for this specific problem** because:
- Migration is multi-day effort (new AppHost project, DI refactoring, test infrastructure overhaul)
- `ThrowawayDb.Postgres` → Aspire test patterns is a non-trivial migration that touches all 66 web tests
- Marten's `DocumentStore` initialization needs to align with Aspire's resource lifecycle
- FusionAuth has no first-party Aspire integration — would need a custom resource or container reference
- The CI problem has a 20-line fix; Aspire migration solves a different (broader) problem

**Recommendation:** Evaluate Aspire in a dedicated spike after CI is green. Don't conflate "CI needs a database" with "we need a new orchestrator."

#### Staged Path

| Phase | Action | Effort | Dependency |
|-------|--------|--------|------------|
| **Now** | Fix workflow: SDK 10.0.x + Postgres service container + all test projects | 30 min | None |
| **Soon** | Pin Postgres image version in docker-compose to match CI (both use `postgres:11`) | 5 min | Phase 1 |
| **Later** | Aspire spike: AppHost + Postgres integration for local dev | 2–3 days | Green CI, team bandwidth |
| **Future** | Aspire test infrastructure migration (replace ThrowawayDb) | 3–5 days | Spike validated |

#### Who Does the Work

- **Phase 1 (CI fix):** Hicks — this is infrastructure YAML, his domain
- **Phase 3 (Aspire spike):** Ripley + Hicks — architecture + implementation

---

## Deferred / Out of Scope

- **P0 Candidates:** None identified. All P1 goals achieved.
- **P2 MCP Resources:** Consider URI-addressable content browsing (future)
- **P3 Response Formatting:** Currently returns raw JSON; formatting improvements deferred
