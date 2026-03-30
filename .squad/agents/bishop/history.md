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
