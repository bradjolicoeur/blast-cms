# Decision: MCP Server Bearer Token Passthrough Authentication

**Author:** Hicks (Backend Dev)  
**Date:** 2026-03-31  
**Status:** Implemented  
**Requested by:** Brad Jolicoeur

---

## Context

The MCP server previously used a two-key authentication model:
1. `MCP_API_KEY` — Bearer token that clients presented to access the `/mcp` endpoint, validated by the MCP server itself
2. `BLAST_CMS_API_KEY` — API key sent as the `ApiKey` header on all downstream requests to the Blast CMS API

This created operational overhead (two secrets to manage, two env vars to configure) and an unnecessary authentication layer at the MCP server.

---

## Decision

**Replace the two-key model with a single-credential passthrough model.**

### New Authentication Flow

1. Client configures their Blast CMS API key as a Bearer token: `Authorization: Bearer <blast-cms-api-key>`
2. MCP server extracts the Bearer token from the incoming request
3. MCP server forwards it as the `ApiKey` header on all outgoing requests to blast-cms
4. blast-cms validates the key using its existing `ApiKeyAttribute` / `ApiKeyFullAttribute` authorization handlers

### Implementation

**New file:** `src/blastcms.McpServer/BearerPassthroughHandler.cs`

A `DelegatingHandler` that:
- Injects `IHttpContextAccessor` to read the current HTTP request context
- Extracts the Bearer token from the `Authorization` header (if present)
- Adds it as the `ApiKey` header on the outgoing HttpClient request to blast-cms

**Program.cs changes:**
- Removed `cmsApiKey` and `mcpApiKey` configuration reads
- Removed the entire bearer auth middleware block (lines 37-66 in old version)
- Added `builder.Services.AddHttpContextAccessor()`
- Registered `BearerPassthroughHandler` as transient
- Chained the handler to the HttpClient: `.AddHttpMessageHandler<BearerPassthroughHandler>()`

### Environment Variables

**Before:**
- `BLAST_CMS_API_KEY` (required)
- `BLAST_CMS_BASE_URL` (required)
- `MCP_API_KEY` (recommended)

**After:**
- `BLAST_CMS_BASE_URL` (required)
- *(No authentication secrets at the MCP layer)*

### Client Configuration

**Before:**
```json
{
  "headers": {
    "Authorization": "Bearer <mcp-api-key>"
  }
}
```

**After:**
```json
{
  "headers": {
    "Authorization": "Bearer <blast-cms-api-key>"
  }
}
```

Clients now use their Blast CMS API key as the Bearer token. The MCP server transparently forwards it to blast-cms.

---

## Rationale

### Benefits

1. **Simpler deployment** — only one env var (`BLAST_CMS_BASE_URL`) needed; no secrets stored at the MCP layer
2. **Single credential** — users manage one API key, not two
3. **Natural extension model** — the MCP server is logically an extension of the blast-cms API surface, so using the same credential makes sense
4. **Per-user authorization** — blast-cms already implements full/read-only key types; this model lets each MCP user authenticate with their own CMS key and inherit their permission level (write tools will 401 for read-only keys)

### Trade-offs

1. **Breaking change** — existing deployments must update:
   - Remove `BLAST_CMS_API_KEY` and `MCP_API_KEY` env vars from Cloud Run service
   - Update client configs to use blast-cms API key as Bearer token
2. **No MCP-level auth** — if you want to restrict MCP access separately from CMS access, you now need to use network-level controls (Cloud Armor, VPC, etc.) instead of a separate Bearer token
3. **HttpContext dependency** — `BearerPassthroughHandler` relies on `IHttpContextAccessor`, which is a standard ASP.NET Core pattern but adds a slight coupling to the request pipeline

The simplification and per-user auth benefits outweigh the trade-offs.

---

## Verification

- **Build:** ✅ 0 errors, 0 warnings
- **Tests:** ✅ All 77 tests passing (no test changes needed; existing MCP integration tests use mocked HttpClient and don't exercise the auth layer)
- **Manual testing:** Not performed yet — will be verified in staging deployment

---

## Migration Path

For existing deployments:

1. Update Cloud Run service env vars:
   ```bash
   gcloud run services update blastcms-mcp \
     --region us-central1 \
     --clear-env-vars BLAST_CMS_API_KEY,MCP_API_KEY
   ```

2. Notify users to update their MCP client configs:
   - Replace `<mcp-api-key>` with their personal Blast CMS API key
   - No other config changes needed

3. Deploy the new MCP server image

---

## Related

- **Commit:** ba2c201
- **Files Changed:** `BearerPassthroughHandler.cs` (new), `Program.cs`, `McpServerUserGuide.md`
- **Documentation:** Full client config examples updated in `McpServerUserGuide.md`
