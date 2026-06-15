# Hard Rules

❌ Never generate or apply migrations — always the developer's responsibility
❌ Never add `Co-Authored-By` to commits — the sole author is Hugo

## Migrations

```
// ❌ Forbidden
dotnet ef migrations add ...
dotnet ef database update
```

When creating a new module with a DbContext, document the expected schema — the developer generates the migration manually.

## Git

```
// ❌ Forbidden — never include this line in commit messages
Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
```
