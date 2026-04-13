---
name: mcp-docs-runtime-alignment
description: Derive MCP client setup docs from runtime transport, path, auth, and local host wiring
domain: documentation, backend
confidence: high
source: earned (Blast CMS README VS Code MCP setup update)
---

## When to use

Use this when updating README or onboarding docs for an MCP server and there is a risk that older setup notes no longer match the actual server behavior.

## Pattern

- Treat the server runtime as the source of truth for MCP setup docs.
- Verify four things before documenting client configuration:
  1. transport type (`http`, `stdio`, etc.)
  2. endpoint shape (including tenant/path prefixes)
  3. client auth header/value
  4. local development host/port wiring from compose or deployment config
- Prefer concise README instructions plus a link to a fuller user guide.
- If deployment or compose files still contain stale environment variables, do not repeat them in user-facing setup docs unless the runtime actually consumes them.

## Blast CMS example

- `src/blastcms.McpServer/Program.cs` shows HTTP transport and `/mcp` mapping
- `src/blastcms.McpServer/TenantMiddleware.cs` requires `/{tenant}/mcp`
- `src/blastcms.McpServer/BearerPassthroughHandler.cs` converts `Authorization: Bearer <token>` into downstream `ApiKey`
- `docker-compose.yml` exposes the MCP server on `localhost:8090` and points local traffic at `BLAST_CMS_BASE_URL`

## Why it matters

MCP docs drift easily because client config examples are often copied forward after auth or routing changes. Grounding the docs in runtime code prevents broken onboarding steps and keeps support noise down.
