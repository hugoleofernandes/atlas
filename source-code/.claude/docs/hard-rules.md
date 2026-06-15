# Hard Rules

❌ Never generate or apply migrations — always the developer's responsibility
❌ Never add `Co-Authored-By` to commits — the sole author is Hugo
✅ When answering after analysis or implementation, explicitly list which instruction docs were read for that task
❌ Never omit the instruction docs read when the task depended on repo rules or `.claude/docs/`

## Migrations

```text
// ❌ Forbidden
dotnet ef migrations add ...
dotnet ef database update
```

When creating a new module with a DbContext, document the expected schema — the developer generates the migration manually.

## Git

```text
// ❌ Forbidden — never include this line in commit messages
Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
```
