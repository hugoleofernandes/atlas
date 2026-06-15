# Query Handlers & Readers

## Rules

✅ Every QueryHandler owns exactly one Reader — 1:1, never shared between handlers
✅ Every QueryHandler owns its own DTO — never reused between handlers
✅ Reader lives in `Atlas.{Module}.Infrastructure`, co-located in the handler's folder
✅ All readers use Dapper with raw SQL — never EF Core
✅ SQL columns: snake_case without quotes; multi-word columns require an explicit alias
✅ Parameters always via anonymous object: `new { TenantId = tenantId }`
✅ Use named arguments when constructing DTOs from query results
✅ For 1:N with pagination: two separate queries, group in C#
✅ Optional filters: build the WHERE clause dynamically — add only predicates for values that were provided
✅ Keep tenant filter explicit in SQL even when automatic safeguards exist
✅ Reader may project derived fields, but should call a pure domain rule when that field represents a real business decision
❌ Never return domain objects from a QueryHandler — always DTOs
❌ Never call a Reader from a CommandHandler
❌ Never JOIN a paginated query when the N side can multiply rows
❌ Never use `@Param IS NULL OR column = @Param` — breaks index usage and Npgsql type inference
❌ Never read another module's schema from a module-owned query — schemas are module boundaries
❌ Never recreate domain business rules inline in SQL, readers, or response mappers when the same rule already exists as a pure domain method

## Folder Structure

```
{Feature}/Handlers/Queries/{QueryName}/
├── I{QueryName}QueryHandler.cs
├── {QueryName}QueryHandler.cs
├── {QueryName}Query.cs
├── I{QueryName}Reader.cs     ← exclusive to this handler (defined in Application)
├── {QueryName}Reader.cs      ← implementation in Infrastructure, same subfolder
└── {QueryName}Dto.cs         ← exclusive to this handler
```

## Column Aliases

Single-word columns (`id`, `name`, `code`) work without alias. Multi-word columns must have one:

```sql
-- ✅ single-word: no alias needed; ✅ multi-word: alias required
SELECT id, name, is_system AS IsSystem, created_at AS CreatedAt
FROM atlas_identity.roles

-- ❌ quoted — never quote snake_case columns
SELECT "Id", "TenantId" FROM atlas_identity.roles

-- ❌ missing alias — Dapper won't map is_system → IsSystem
SELECT id, name, is_system FROM atlas_identity.roles
```

## 1:N Pagination

```csharp
// ✅ two queries — never JOIN when the N side multiplies rows
var roles = await conn.QueryAsync<RoleRow>(RolesSql, new { TenantId = tenantId });
var permissions = await conn.QueryAsync<PermissionRow>(
    PermissionsSql, new { RoleIds = roles.Select(r => r.Id).ToArray() });
var lookup = permissions.ToLookup(p => p.RoleId);
```

## DTO Construction

```csharp
// ✅ named arguments — safe when properties are added or reordered
return new RoleDto(
    RoleId:          role.Id,
    Name:            role.Name,
    IsSystem:        role.IsSystem,
    PermissionCodes: lookup[role.Id].Select(p => p.Code).ToList());

// ❌ positional — breaks silently when DTO shape changes
return new RoleDto(role.Id, role.Name, role.IsSystem, permissions);
```

## Dynamic SQL Filters

Optional filters must be appended conditionally — never with an `IS NULL OR` guard:

```csharp
// ✅ dynamic WHERE — index-friendly, Npgsql-safe
var sql = new StringBuilder("SELECT ... FROM atlas_platform.audit_log WHERE tenant_id = @TenantId");
var parameters = new DynamicParameters(new { TenantId = tenantId });

if (query.Action is not null)
{
    sql.AppendLine("  AND action = @Action");
    parameters.Add("Action", query.Action);
}

// ❌ OR-guard — disables the index, fails Npgsql type inference for nullable params
// AND (@Action IS NULL OR action = @Action)
```

This matters most for high-growth tables (audit log, outbox) where index usage is critical.

## Repeated SQL Predicates

When the same condition appears in both SELECT and WHERE, extract it as a constant:

```csharp
private const string IsActivePredicate = "NOT i.is_used AND i.expires_at >= @Now";

private const string Sql = $"""
    SELECT ({IsActivePredicate}) AS IsActive, ...
    WHERE (@IncludeActive AND ({IsActivePredicate}))
    """;
```

## Derived Fields

Readers may return DTO fields derived from the query result, but there is an important split of responsibility:

- data shaping belongs to the reader
- business rule ownership belongs to the domain

Preferred pattern:

```csharp
CanResubmit: OutboxMessage.CanBeResubmitted(
    isDeadLettered: row.Status == "DeadLettered",
    hasReplayChild: row.HasReplayChild)
```

Avoid re-encoding business decisions directly in the reader:

```csharp
CanResubmit: row.Status == "DeadLettered" && !row.HasReplayChild
```

That keeps the rule centralized while still allowing lightweight DTO projection without loading aggregates.
