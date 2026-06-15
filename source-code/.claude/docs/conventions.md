# Conventions

## Code Formatting

The project uses **CSharpier** as the formatter — the C# equivalent of Prettier.

- Extension installed in Visual Studio with **Format on Save** enabled
- Configuration in `.csharpierrc.json` at the repository root (`printWidth: 120`)
- `.editorconfig` complements with style rules (indentation, line endings, `var`, modifiers)

Never adjust formatting manually — saving the file already formats it. If code looks "unformatted" before saving, that's normal.

## Pagination

**The project default is no pagination** — endpoints return `IReadOnlyList<T>` directly. Server-side pagination is only added when explicitly requested. Never add `Page`/`PageSize` on your own initiative.

## Health Checks

Both health check endpoints live in `Atlas.API/Program.cs` — never in individual modules.

```csharp
// /health/live  → process running (no DB touch) — livenessProbe
// /health/ready → Postgres reachable — readinessProbe
app.MapHealthChecks("/health/live",  new HealthCheckOptions { Predicate = c => c.Tags.Contains("live"),  ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse }).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready"), ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse }).AllowAnonymous();
```

Packages: `AspNetCore.HealthChecks.NpgSql` and `AspNetCore.HealthChecks.UI.Client` (version 9.x).

## ErrorCategory → HTTP Status Mapping

The single source of truth is `ErrorCategoryExtensions.ToHttpStatus()` in `Atlas.BuildingBlocks.AspNetCore.HttpErrors`.

All consumers use the same extension: `GlobalExceptionMiddleware`, `HttpResultMapper`, and endpoints via `AtlasEndpoint`. Never duplicate the `ErrorCategory → int` switch anywhere else.
