# Decision: CI Test Gate Before Docker Publish

**Author:** Hicks (Backend Dev)
**Date:** 2026-03-31
**Status:** Implemented
**Requested by:** Brad Jolicoeur

---

## Context

The GitHub Actions workflow (`github-actions-push.yml`) was building and publishing Docker images without running any tests first. A broken commit could ship straight to the test environment and block deployments, with no automated safety net.

---

## Decision

Add a `run-tests` job as the first job in the push workflow. Both `build-and-publish` and `build-and-publish-mcp` must succeed `run-tests` before they execute.

---

## Implementation

**New job added:**
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

**Build jobs updated:**
- `build-and-publish`: added `needs: run-tests`
- `build-and-publish-mcp`: added `needs: run-tests`

**Unchanged:** all deploy jobs, env vars, Docker steps.

---

## Resulting Workflow

```
run-tests ──→ build-and-publish   ──→ deploy-test ──→ deploy-production
          └──→ build-and-publish-mcp ──→ deploy-mcp-test ──→ deploy-mcp-production
```

---

## Rationale

- Single shared test job avoids duplication and ensures one consistent test run gates both pipelines
- Failing tests now block all downstream work — nothing gets published if tests are red
- Test project (`blastcms.web.tests`) covers the main web app; currently 66 tests passing
- No friction added to passing builds — test job runs in parallel with nothing else, so it doesn't lengthen total workflow time beyond the test duration itself

---

## Commit

`ffdcc85` — ci: add test gate before Docker publish
