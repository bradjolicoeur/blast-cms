# GitHub Actions Postgres-backed .NET tests

## When to use

Use this pattern when a .NET test project creates databases dynamically (for example with ThrowawayDb or Marten) and assumes a PostgreSQL server is already reachable on `localhost`.

## Pattern

Add a PostgreSQL service container to the GitHub Actions test job instead of booting the whole local compose stack:

```yaml
jobs:
  run-tests:
    runs-on: ubuntu-latest
    env:
      DB_HOST: localhost
    services:
      postgres:
        image: postgres:11
        env:
          POSTGRES_USER: blastcms_user
          POSTGRES_PASSWORD: not_magical_scary
          POSTGRES_DB: blastcms_database
        ports:
          - 5432:5432
        options: >-
          --health-cmd "pg_isready -U blastcms_user -d blastcms_database"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
    steps:
      - uses: actions/checkout@v2
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet restore src\solution.sln
      - run: dotnet test src\primary-db-tests.csproj --no-restore --verbosity normal
      - run: dotnet test src\other-tests.csproj --no-restore --verbosity normal
```

## Why it works

- It matches the local dependency shape closely enough for tests that only need PostgreSQL.
- It is smaller and more reliable than starting unrelated services.
- It avoids coupling a narrow CI fix to a broader orchestration migration such as Aspire.

## blast-cms example

- `src\blastcms.web.tests\OneTimeStartup.cs` reads `DB_HOST` and creates disposable databases with `ThrowawayDb.Postgres`.
- `src\blastcms.McpServer.Tests` and `src\blastcms.FusionAuthService.Tests` do not need live Postgres, but should still run in the same CI gate.
- Restoring `src\blastcms.sln` once and then running each test project with `--no-restore` keeps the workflow explicit and avoids duplicate restore work.
