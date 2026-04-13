---
name: aspnet-mcp-tenant-routing
description: Prevent false 404s when tenant middleware rewrites ASP.NET Core MCP paths
domain: backend, mcp, aspnet
confidence: high
source: earned (Blast CMS tenant-prefixed MCP 404 investigation)
---

## When to use

Use this when an ASP.NET Core MCP server exposes tenant-prefixed URLs like `/{tenant}/mcp`, but the actual MCP endpoints are mapped at `/mcp` and a middleware rewrites the request path.

## Pattern

- If middleware rewrites the request path before an endpoint like `app.MapMcp("/mcp")`, make routing order explicit.
- In minimal hosting, implicit endpoint routing can run before user middleware.
- When that happens, rewriting `/{tenant}/mcp` to `/mcp` is too late and requests fall through as `404`.

## Fix

Place the rewrite middleware before an explicit `UseRouting()` call:

```csharp
app.UseMiddleware<TenantMiddleware>();
app.UseRouting();
app.MapMcp("/mcp");
```

## Blast CMS example

- `src/blastcms.McpServer/TenantMiddleware.cs` rewrites `/{tenant}/mcp[/...]` to `/mcp[/...]`
- `src/blastcms.McpServer/Program.cs` maps the MCP endpoint with `app.MapMcp("/mcp")`
- Before the fix, live and local `/{tenant}/mcp` requests returned `404`
- After the fix, the same path reached the MCP endpoint and returned protocol-level responses instead of route-not-found

## Why it matters

This failure looks like bad client configuration, but the client URL can be perfectly correct. Verifying the difference between bare `/mcp` and `/{tenant}/mcp` is a fast way to distinguish doc/config issues from ASP.NET Core routing-order bugs.
