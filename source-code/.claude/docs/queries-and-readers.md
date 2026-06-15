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
❌ Never return domain objects from a QueryHandler — always DTOs
❌ Never call a Reader from a CommandHandler
❌ Never JOIN a paginated query when the N side can multiply rows

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

## Repeated SQL Predicates

When the same condition appears in both SELECT and WHERE, extract it as a constant:

```csharp
private const string IsActivePredicate = "NOT i.is_used AND i.expires_at >= @Now";

private const string Sql = $"""
    SELECT ({IsActivePredicate}) AS IsActive, ...
    WHERE (@IncludeActive AND ({IsActivePredicate}))
    """;
```
