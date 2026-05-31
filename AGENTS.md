# Codex Instructions

This file contains repository-specific instructions for Codex/OpenAI coding agents.
Codex should read and follow these rules when analyzing, editing, or reviewing this
codebase. Use it to capture project conventions, architectural decisions, and
performance/security constraints that must survive across sessions.

## Database Query Performance

- Do not write optional SQL filters using `(@Param IS NULL OR column = @Param)` or similar `OR`-based null guards.
- Prefer building the `WHERE` clause dynamically and adding only the predicates for filters that were provided.
- This is especially important for audit trail queries and other high-growth tables, where indexes must remain usable.
- For PostgreSQL/Npgsql with Dapper, nullable parameters in `@Param IS NULL OR ...` expressions can also fail type inference.
- Keep tenant filters explicit in queries, even when automatic tenant safeguards exist.
- Do not read another module's schema from a module-owned query. Even in the modular monolith,
  schemas are module boundaries and may become separate databases/services later.

Preferred pattern:

```csharp
if (query.Action is not null)
{
    sql.AppendLine("  AND action = @Action");
    parameters.Add("Action", query.Action);
}
```

Avoid:

```sql
AND (@Action IS NULL OR action = @Action)
```
