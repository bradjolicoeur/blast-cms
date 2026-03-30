# Project Context

- **Owner:** Brad Jolicoeur
- **Project:** blast-cms — a headless content management system built on .NET with Blazor Server. Websites consume content via REST API. Admin interface is Blazor Server.
- **Stack:** .NET (C#), Blazor Server, REST API, FusionAuth for identity, Google Cloud Storage for assets, MCP server for AI integrations. Multi-project solution at `src/`.
- **Created:** 2026-03-30

## Key Projects

- `blastcms.web` — Blazor Server admin UI
- `blastcms.handlers` — REST API request handlers / endpoints
- `blastcms.ArticleScanService` — background service for article scanning
- `blastcms.ImageResizeService` — background service for image resizing
- `blastcms.FusionAuthService` — FusionAuth identity integration
- `blastcms.McpServer` — MCP server for AI tool integrations
- `blastcms.UserManagement` — user management layer
- Test projects: `blastcms.web.tests`, `blastcms.McpServer.Tests`, `blastcms.FusionAuthService.Tests`

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->
