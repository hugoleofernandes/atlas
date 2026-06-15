# Request Context

## Rules

✅ Handlers inject `IRequestContext` — read-only access to tenant/user
✅ Always guard `TenantId` with `?? throw new TenantContextNotResolvedException()`
✅ Middleware, seeders, and the Outbox worker inject `IRequestContextSetter` to populate the context
✅ Outbox worker uses `WorkerRequestContext` — a separate implementation populated from outbox message metadata
❌ Never inject `IRequestContextSetter` into a handler — handlers only read the context
❌ Never assume the context is populated — always guard before using `TenantId` or `UserId`

## Two Interfaces, One Instance

| Interface | Who uses it | What it does |
|---|---|---|
| `IRequestContext` | Handlers, repositories, audit pipeline | Read tenant/user identity |
| `IRequestContextSetter` | Middleware, seeders, Outbox worker | Populate the context |

Both resolve to the same scoped `RequestContext` instance within a DI scope.

## What It Carries

```csharp
public interface IRequestContext
{
    bool    IsAuthenticated;
    Guid?   TenantId;
    string? TenantName;
    Guid?   UserId;
    string? UserEmail;
    string? CorrelationId;
    bool    TenantFilterSuspended;
}
```

## Handler Pattern

```csharp
public async Task<InviteUserOutput> ExecuteAsync(InviteUserCommand cmd, CancellationToken ct)
{
    // ✅ always guard — context may not be populated
    var tenantId = _requestContext.TenantId ?? throw new TenantContextNotResolvedException();
}
```

## Special Cases

**Suspend filter** — when TenantId is not yet known (e.g. `ResolveTenantAccess`):
```csharp
using (_contextSetter.SuspendTenantFilter())
{
    var user = await _userRepository.FindActiveByEmailAsync(email, ct);
} // filter restored automatically
```

**Seeders** — no HTTP request, set manually before saving:
```csharp
setter.Set(tenant.Id, tenant.Name, SystemIdentity.UserId, SystemIdentity.Email);
await uow.SaveChangesAsync(ct);
```
