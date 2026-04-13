# Squad Decisions

## Active Decisions

### Decision: MCP Server Architectural Evaluation

**Author:** Ripley (Lead)  
**Date:** 2026-03-30  
**Status:** Assessment — awaiting team action  
**Requested by:** Brad Jolicoeur

---

## Context

Brad asked for an evaluation of `src/blastcms.McpServer` against two requirements:

1. **Users can easily register the MCP server** to help them create and maintain content.
2. **The MCP should help agents create websites** that consume and display the content.

---

## Requirement 1: Easy Registration

### What Exists Today

**Verdict: MEETS the requirement.**

The registration story is strong. Here's what's in place:

- **Remote HTTP transport** (`Program.cs:21–23`) — no local install, no stdio, no Docker needed on the client side. Users point their AI client at a URL. This is the simplest possible registration model.
- **`McpServerUserGuide.md`** — 325 lines of clear, client-specific setup instructions covering Claude Desktop, VS Code Copilot (workspace and user-level), and Gemini CLI. Includes config file paths per OS, JSON snippets, verification steps, example prompts, and troubleshooting.
- **Bearer token auth** (`Program.cs:32–59`) — optional, constant-time comparison, well-documented. Users either supply an `Authorization` header or omit it for open-access deployments.
- **Cloud Run deployment docs** — full `gcloud run deploy` command, Dockerfile, docker-compose for local dev.

### What's Missing

- **Minor:** No Copilot CLI (`~/.copilot/mcp-config.json`) registration example in the user guide. This is the tool Brad is using right now. Easy to add.
- **Minor:** The README.md doesn't mention the MCP server at all. A one-liner pointing to `McpServerUserGuide.md` would help discoverability.

### Gap Severity: Minor

Registration is well-handled. The gaps are documentation polish, not architecture.

---

## Requirement 2: Help Agents Create Websites That Consume and Display Content

### What Exists Today

**Verdict: PARTIALLY MEETS — significant gaps.**

The MCP server exposes **9 read-only tools** across 3 entity types:

| Tool Class | Tools | Entity Type |
|------------|-------|-------------|
| `BlogArticleTools` | `list_blog_articles`, `get_blog_article_by_slug`, `get_blog_article_by_id` | BlogArticle |
| `ContentBlockTools` | `list_content_blocks`, `get_content_block_by_slug`, `get_content_blocks_by_group`, `get_content_block_by_id` | ContentBlock |
| `FeedArticleTools` | `list_feed_articles`, `get_feed_article_by_id` | FeedArticle |

These tools let an agent **read** blog articles, content blocks, and feed articles — enough to scaffold a basic blog template that pulls content from the CMS API.

### What's Missing

#### Gap 1: No Write Tools — BLOCKING for "create and maintain content"

Every tool is a GET proxy. There are zero create, update, or delete tools. The CMS REST API already supports `POST /api/blogarticle/`, `POST /api/contentblock/`, etc. — the MCP server just doesn't expose them.

Without write tools, an agent **cannot create or maintain content** through MCP. Requirement 1 explicitly says "create and maintain." This is a blocking gap.

**Priority: P0 — add create/update tools for at least BlogArticle and ContentBlock.**

#### Gap 2: Only 3 of 15 Entity Types Covered — SIGNIFICANT

The CMS REST API has **48 endpoints across 13 controllers** covering **15 entity types**. The MCP server exposes only 3 types. Missing entirely:

- **Podcast / PodcastEpisode** — 7 API endpoints, 0 MCP tools
- **Event / EventVenue** — 8 API endpoints, 0 MCP tools  
- **LandingPage** — 3 API endpoints, 0 MCP tools
- **ImageFile** — 5 API endpoints, 0 MCP tools
- **ContentTag** — 2 API endpoints, 0 MCP tools
- **UrlRedirect** — 3 API endpoints, 0 MCP tools
- **SitemapItem** — 2 API endpoints, 0 MCP tools
- **EmailTemplate** — 1 API endpoint, 0 MCP tools

For "help agents create websites," an agent needs LandingPage, ImageFile, and ContentTag at minimum — these are core to building a site. Podcast and Event coverage depends on whether the target site uses those features.

**Priority: P1 — add read tools for LandingPage, ImageFile, ContentTag. Others can follow.**

#### Gap 3: No MCP Resources — MINOR

The server uses only MCP Tools (method calls). It doesn't define any MCP Resources (URI-addressable content). Resources would let agents browse the CMS content tree more naturally (e.g., `blast-cms://articles/my-first-post`). This is a nice-to-have, not a blocker — the tools work fine for now.

**Priority: P2 — consider later.**

#### Gap 4: Raw JSON Responses — MINOR

All tools return raw JSON strings from the CMS API (`response.Content.ReadAsStringAsync()`). No transformation, no structured typing. This works — LLMs handle JSON fine — but richer tool metadata or summary formatting could improve agent comprehension for large result sets.

**Priority: P3 — cosmetic improvement.**

---

## Test Coverage Assessment

`src/blastcms.McpServer.Tests/McpServerTests.cs` has **10 integration tests** using in-process stream transport with mocked HTTP handlers:

- ✅ Tool registration verification (3 tests — one per tool class)
- ✅ Successful invocation for list operations (3 tests)
- ✅ Successful invocation for get-by-slug (1 test)
- ✅ 401 error propagation (3 tests)
- ❌ No tests for get-by-id or get-by-group tools
- ❌ No tests for search/tag filter parameters
- ❌ No write tool tests (none exist to test)

The test infrastructure is solid — the `CreateClientServerPair` helper is well-designed. Coverage is adequate for the current read-only surface but will need expansion as tools are added.

---

## Overall Verdict

| Requirement | Status | Summary |
|-------------|--------|---------|
| R1: Easy registration | ✅ **Met** | Remote HTTP, excellent user guide, minor doc gaps |
| R2: Help agents create websites | ⚠️ **Partially met** | Read-only tools for 3/15 entity types. No write capability. |

### Top Priorities to Close the Gap

1. **P0 — Add write tools** for BlogArticle and ContentBlock (create/update). This is the single biggest gap — without it, "create and maintain content" is not possible through MCP.
2. **P1 — Add read tools** for LandingPage, ImageFile, and ContentTag. These are essential for website-building use cases.
3. **P1 — Add read tools** for Podcast/PodcastEpisode and Event/EventVenue based on target site needs.
4. **P2 — User guide additions:** Copilot CLI config example, README cross-reference.
5. **P3 — Consider MCP Resources** and response formatting improvements.

### Architecture Quality

The architecture itself is clean and correct:
- Transport choice (Streamable HTTP) is right for a remote deployment
- Auth middleware is secure (constant-time comparison, optional)
- Tool pattern is consistent and follows MCP SDK conventions
- Test infrastructure is well-structured
- Deployment story (Cloud Run + Docker Compose) is complete

The issue isn't how it's built — it's how much is built. The foundation is solid; the surface area needs to grow.

---

## Decision: CI Test Gate Before Docker Publish

**Author:** Hicks (Backend Dev)  
**Date:** 2026-03-31  
**Status:** Implemented  
**Requested by:** Brad Jolicoeur

### Context

The GitHub Actions workflow (`github-actions-push.yml`) was building and publishing Docker images without running any tests first. A broken commit could ship straight to the test environment and block deployments, with no automated safety net.

### Implementation

Added a `run-tests` job as the first job in the push workflow. Both `build-and-publish` and `build-and-publish-mcp` must succeed `run-tests` before they execute.

```yaml
run-tests:
  runs-on: ubuntu-latest
  steps:
    - uses: actions/checkout@v2
    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    - run: dotnet restore src/blastcms.web.tests/blastcms.web.tests.csproj
    - run: dotnet test src/blastcms.web.tests/blastcms.web.tests.csproj --no-restore --verbosity normal
```

**Resulting Workflow:**
```
run-tests ──→ build-and-publish   ──→ deploy-test ──→ deploy-production
          └──→ build-and-publish-mcp ──→ deploy-mcp-test ──→ deploy-mcp-production
```

### Rationale

- Single shared test job avoids duplication and ensures one consistent test run gates both pipelines
- Failing tests now block all downstream work — nothing gets published if tests are red
- Test project (`blastcms.web.tests`) covers the main web app; currently 66 tests passing
- No friction added to passing builds — test job runs in parallel with nothing else

**Commit:** `ffdcc85`

---

## Decision: MCP In-Process Test Harness Scoped Dependencies

**Author:** Bishop (Tester)  
**Date:** 2026-04-13  
**Status:** Implemented  
**Requested by:** Brad Jolicoeur

### Context

After tenant-aware MCP routing landed in `blastcms.McpServer`, all MCP tools now depend on a scoped `TenantContext` service. The in-process test hosts in `src/blastcms.McpServer.Tests/*.cs` were not registering this dependency, causing all tool invocation tests to fail while tool discovery tests passed.

### Decision

When a scoped request service is introduced in `blastcms.McpServer`, update all `CreateClientServerPair` helpers in the test assembly to register that dependency with a deterministic test value before diagnosing bulk failures as product regressions.

### Pattern

```csharp
services.AddScoped(_ => new TenantContext { TenantId = "test-tenant" });
```

### Application

Applied to all seven test files in `src/blastcms.McpServer.Tests/`:
- `McpServerTests.cs`
- `ApiKeyHeaderTests.cs`
- `ContentTagToolTests.cs`
- `ImageFileToolTests.cs`
- `LandingPageToolTests.cs`
- `McpServerWriteToolTests.cs`
- `PodcastToolTests.cs`

### Result

- All 22 failing tests resolved
- Full solution test suite passed at 134/134
- Test infrastructure now mirrors production DI contract

### Impact

Future scoped service additions to `blastcms.McpServer` must follow this pattern; otherwise, invocation failures can be misclassified as product regressions when discovery tests will continue to pass.

---

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
