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

## Enum Serialization in JSON Requests

✅ Enums serialize/deserialize as strings — `DescriptiveEnumJsonConverter` is registered globally in `UseFastEndpoints` (`Atlas.API/Program.cs`)
✅ When an unknown string value is received, the response is a 400 with a human-readable message listing the valid values
❌ Never add `[JsonConverter(typeof(JsonStringEnumConverter))]` on individual enum types — the global converter already covers all enums
❌ Never replace `DescriptiveEnumJsonConverter` with the default `JsonStringEnumConverter` — it produces a cryptic error that exposes internal type names instead of valid values

Valid values for any enum are always the exact member names (PascalCase). When an invalid value is received the response lists them: `"Invalid value 'X' for EnumName. Valid values: A, B, C."`.

When adding a new enum used in a request, no extra registration is needed — `DescriptiveEnumJsonConverter` handles all enums automatically.
