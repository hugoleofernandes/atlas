# Multi-tenancy

## Rules

✅ Entities that belong to a tenant implement `IMultiTenantEntity` — global filter applied automatically
✅ Entities that are cross-tenant (e.g. `Tenant` itself) implement `INotMultiTenant`
✅ Cross-tenant reads use `IgnoreQueryFilters()` on the EF query
✅ Suspend the filter with `using (_contextSetter.SuspendTenantFilter())` when `TenantId` is not yet known
❌ Never use `IgnoreQueryFilters()` in a regular handler — only in flows that genuinely need cross-tenant access
❌ Never forget to restore the filter — always use `using` so it reactivates on scope exit

## Entity Marker Interfaces

```csharp
// ✅ tenant-scoped — global filter applies; only current tenant's rows returned
public sealed class Invitation : AggregateRoot, IMultiTenantEntity { ... }

// ✅ cross-tenant — no filter; all rows always visible
public sealed class Tenant : AggregateRoot, INotMultiTenant { ... }
```

## Suspend Filter Pattern

Use when `TenantId` is not yet resolved — e.g. `ResolveTenantAccess`, which *is* the flow that discovers the tenant:

```csharp
using (_contextSetter.SuspendTenantFilter())
{
    var user = await _userRepository.FindActiveByEmailAsync(email, ct);
} // filter restored automatically on using exit
```

## Cross-Tenant Read (Seeders & Admin Flows)

```csharp
// ✅ bypass filter without suspending — for targeted cross-tenant queries
var exists = await db.Users
    .IgnoreQueryFilters()
    .AnyAsync(u => u.Id == BootstrapIdentity.RootUser.Id, ct);
```

`SuspendTenantFilter()` affects the entire scope. `IgnoreQueryFilters()` is per-query — prefer it when only one query needs cross-tenant access.
