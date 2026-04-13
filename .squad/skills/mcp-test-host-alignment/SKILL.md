---
name: mcp-test-host-alignment
description: Keep in-process MCP test hosts aligned with runtime DI when tool dependencies change
domain: testing, backend
confidence: high
source: earned (Blast CMS MCP tenant-routing regression)
---

## When to use

Use this when `src/blastcms.McpServer.Tests` builds an in-process MCP server with `WithStreamServerTransport(...)` and tool calls suddenly start returning MCP errors after backend wiring changes.

## Pattern

- Mirror production DI in the test host, not just the tool registrations.
- If an MCP tool constructor adds a scoped dependency, register that dependency in every test fixture that creates the server.
- Use a deterministic test value for request-scoped context objects such as `TenantContext`.

## Blast CMS example

Tenant-aware MCP routing added `TenantContext` to tool constructors. The fix for the 22 failing MCP tests was to add:

```csharp
services.AddScoped(_ => new TenantContext { TenantId = "test-tenant" });
```

to each `CreateClientServerPair(...)` helper under `src/blastcms.McpServer.Tests`.

## Why it matters

MCP tool discovery can still succeed when DI for invocation is broken. Always verify at least one real tool call after wiring changes so fixture drift is caught early.
