# Team Decisions — blast-cms

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

## Deferred / Out of Scope

- **P0 Candidates:** None identified. All P1 goals achieved.
- **P2 MCP Resources:** Consider URI-addressable content browsing (future)
- **P3 Response Formatting:** Currently returns raw JSON; formatting improvements deferred
