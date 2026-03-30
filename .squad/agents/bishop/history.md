# Project Context

- **Owner:** Brad Jolicoeur
- **Project:** blast-cms — a headless content management system built on .NET with Blazor Server. Websites consume content via REST API. Admin interface is Blazor Server.
- **Stack:** .NET (C#), Blazor Server, REST API, FusionAuth for identity, Google Cloud Storage for assets, MCP server for AI integrations. Multi-project solution at `src/`.
- **Created:** 2026-03-30

## Key Projects (Test Focus)

- `blastcms.web.tests` — Blazor admin UI tests
- `blastcms.McpServer.Tests` — MCP server tests
- `blastcms.FusionAuthService.Tests` — FusionAuth integration tests

## Test Framework Notes

- xUnit is the primary test framework
- bUnit for Blazor component testing

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026 — MCP Write Tool Tests (McpServerWriteToolTests.cs)

**Test infrastructure pattern:**
- `CreateClientServerPair(HttpMessageHandler)` wires an in-process MCP server with an `McpClient` using `System.IO.Pipelines`. All tool registrations are explicit (`.WithTools<T>()`); tool scanning is not used.
- `IHttpClientFactory` is mocked with Moq. The mock returns a single `HttpClient` with `BaseAddress = http://localhost/` keyed by `BlastCmsClientConstants.HttpClientName`.
- Two handler factories: `CreateSuccessHandler(json, statusCode)` returns a configured 2xx mock; `CreateErrorHandler(statusCode, body)` returns 4xx/5xx mocks. Both use `Moq.Protected` to stub `SendAsync`.
- Tool calls go through `client.CallToolAsync("tool_name", Dictionary<string, object?>)`. Tool names are strings, so tests compile even when the implementing method doesn't exist yet.
- Assertions: check `result.IsError` for error propagation; inspect `result.Content.OfType<TextContentBlock>().FirstOrDefault()` and `.Text` for success content.
- Tool registration tests use `client.ListToolsAsync()` and assert on `t.Name`.

**Coverage decisions for write tools:**
- Tests written against expected tool names: `create_blog_article`, `update_blog_article`, `create_content_block`, `update_content_block`.
- Tests compile against existing assembly; they will fail at runtime until Hicks completes the implementation.
- 201 Created status is handled by `CreateSuccessHandler` with explicit `HttpStatusCode.Created`.
