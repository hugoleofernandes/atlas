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

## Seeding Conventions

- Prefer explicit startup seeding orchestration over implicit `IEnumerable<T>` module discovery when seed order matters.
- Keep each module seeder's top-level `SeedAsync` linear and easy to read.
- If one aggregate seed depends on data produced by another aggregate seed, return a small typed output from the first step and pass it explicitly into the next step.
- Avoid hiding cross-step dependencies through generic mutable state bags when a narrow typed output is enough.
- When splitting a module seeder into partial files, prefer one file per aggregate so the seeding structure mirrors the domain model.
