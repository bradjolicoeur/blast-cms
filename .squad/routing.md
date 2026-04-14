# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Architecture, technical decisions, scoping | Ripley | API design, solution structure, build-vs-buy, cross-cutting concerns |
| REST API, backend services, integrations | Hicks | handlers, ArticleScanService, ImageResizeService, FusionAuth, MCP tools, GCS |
| Blazor Server UI, admin interface | Hudson | blastcms.web pages and components, Razor markup, UI state |
| Tests, quality, edge cases | Bishop | xUnit tests, bUnit component tests, integration test coverage |
| Code review | Ripley | Review PRs, check quality, suggest improvements |
| Issue triage | Ripley | Reads issue, assigns `squad:{member}` label, comments with notes |
| Session logging | Scribe | Automatic — never needs routing |
| Work queue / backlog | Ralph | Monitor GitHub issues and PRs, keep pipeline moving |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, assign `squad:{member}` label | Ripley |
| `squad:ripley` | Architecture/review/scoping work | Ripley |
| `squad:hicks` | Backend service or API work | Hicks |
| `squad:hudson` | Blazor UI work | Hudson |
| `squad:bishop` | Test writing or quality work | Bishop |

### How Issue Assignment Works

1. When a GitHub issue gets the `squad` label, **Ripley** triages it — analyzing content, assigning the right `squad:{member}` label, and commenting with triage notes.
2. When a `squad:{member}` label is applied, that member picks up the issue in their next session.
3. Members can reassign by removing their label and adding another member's label.
4. The `squad` label is the "inbox" — untriaged issues waiting for Ripley's review.

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for "what port does the server run on?"
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **Anticipate downstream work.** If a feature is being built, spawn Bishop to write test cases from requirements simultaneously.
7. **Issue-labeled work** — when a `squad:{member}` label is applied to an issue, route to that member. Ripley handles all `squad` (base label) triage.
