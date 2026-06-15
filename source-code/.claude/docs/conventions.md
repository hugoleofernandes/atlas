# Conventions

## Rules

✅ Formatting is handled by **CSharpier** (`printWidth: 120`) — never adjust manually
✅ Endpoints return `IReadOnlyList<T>` directly — no pagination unless explicitly requested
✅ Health checks live only in `Atlas.API/Program.cs` — never in individual modules
✅ `ErrorCategory → HTTP status` mapping lives only in `ErrorCategoryExtensions.ToHttpStatus()`
❌ Never add `Page`/`PageSize` parameters on your own initiative
❌ Never duplicate the `ErrorCategory → int` switch anywhere else
❌ Never add health check endpoints inside a module

## Formatting

CSharpier formats on save — code may look unformatted before saving, that's normal.
`.editorconfig` at repo root: CRLF line endings, UTF-8, 4-space indent for C#, 2-space for JSON/csproj.
`.gitattributes` enforces CRLF on Git checkout — never commit LF line endings.

## ErrorCategory → HTTP Status

Single source of truth: `ErrorCategoryExtensions.ToHttpStatus()` in `Atlas.BuildingBlocks.AspNetCore.HttpErrors`.
Used by `GlobalExceptionMiddleware`, `HttpResultMapper`, and `AtlasEndpoint`. Never duplicated.
