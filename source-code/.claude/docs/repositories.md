# Repository Pattern & Unit of Work

## Rules

✅ Repository **interfaces** live in `Atlas.{Module}.Application/Repositories/`
✅ Repository **implementations** live in `Atlas.{Module}.Infrastructure/Repositories/`
✅ Implementations use EF Core — never Dapper (Dapper is for Readers only)
✅ Expose `IUnitOfWork UnitOfWork => _uow` on every CommandHandler — the pipeline saves
✅ Method naming: `GetBy...` / `Find...` (returns null), `ExistsWith...` (bool), `AddAsync`, `Remove`
❌ Never put repository interfaces in Domain — Domain never calls repositories, CommandHandlers do
❌ Never call `SaveChangesAsync` inside a repository — that is the UoW's job
❌ Never call `SaveChangesAsync` inside a CommandHandler — the `PersistDbDecorator` does it

## Interface — Application Layer

```csharp
// Atlas.Identity.Application/Repositories/IRoleRepository.cs
public interface IRoleRepository
{
    Task<Role?> GetByIdWithPermissionsAsync(Guid roleId, CancellationToken ct);
    Task<bool> ExistsWithNameAsync(Guid tenantId, string name, CancellationToken ct);
    Task AddAsync(Role role, CancellationToken ct);
    void Remove(Role role);
}
```

## Implementation — Infrastructure Layer

```csharp
// Atlas.Identity.Infrastructure/Repositories/RoleRepository.cs
public sealed class RoleRepository(IdentityDbContext db) : IRoleRepository
{
    public async Task<Role?> GetByIdWithPermissionsAsync(Guid roleId, CancellationToken ct)
        => await db.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Id == roleId, ct);

    public async Task AddAsync(Role role, CancellationToken ct) => await db.Roles.AddAsync(role, ct);
    public void Remove(Role role) => db.Roles.Remove(role);
}
```

## CommandHandler Pattern

```csharp
public sealed class CreateRoleCommandHandler : ICreateRoleCommandHandler
{
    public IUnitOfWork UnitOfWork => _uow;  // ← required; PersistDbDecorator calls SaveChangesAsync

    public async Task<CreateRoleOutput> ExecuteAsync(CreateRoleCommand cmd, CancellationToken ct)
    {
        var tenantId = _requestContext.TenantId ?? throw new TenantContextNotResolvedException();

        if (await _roleRepository.ExistsWithNameAsync(tenantId, cmd.Name, ct))
            throw new DuplicateRoleNameException(cmd.Name);

        var tenant = await _tenantRepository.GetByIdAsync(tenantId, ct)
            ?? throw new TenantNotFoundException(tenantId);

        var role = tenant.AddRole(cmd.Name, cmd.PermissionCodes);
        await _roleRepository.AddAsync(role, ct);

        return new CreateRoleOutput(role.Id, role.Name);
        // PersistDbDecorator calls _uow.SaveChangesAsync() after this returns
    }
}
```

## Why Interfaces Live in Application, Not Domain

Domain aggregates never call repositories — only CommandHandlers do.
The consumer is in Application, so the contract belongs in Application.
Domain stays free of any persistence concept.
