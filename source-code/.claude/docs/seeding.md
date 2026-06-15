# Seeding

## Rules

✅ Every module seeder implements the 4-member contract: `GetModule`, `GetModulePermissions`, `GetModuleEntityTypes`, `SeedAsync`
✅ Each seed step lives in its own partial file — one per aggregate
✅ Every seed step guards with `AnyAsync()` at the aggregate level — all-or-nothing, never row-by-row
✅ Always use `IUnitOfWork.SaveChangesAsync()` — never `db.SaveChangesAsync()` directly
✅ Cross-tenant reads must use `IgnoreQueryFilters()` — the global filter is active in seeders
✅ Log: `started` → work → `completed — {summary}` (or `skipped - data already exists`)
❌ Never call `db.SaveChangesAsync()` directly — bypasses the audit pipeline
❌ Never guard row-by-row — if data exists at the aggregate level, skip the entire step

## Two Layers

| Layer | Class | Location |
|---|---|---|
| Global orchestrator | `AtlasBootstrapSeeder` | `Atlas.API/Seeding/` |
| Module seeder | `{Module}ModuleSeeder` | `Atlas.{Module}.Infrastructure/Seeders/` |

## Module Seeder Contract

```csharp
// {Module}ModuleSeeder.cs — main file: contract + SeedAsync sequence only
public sealed partial class IdentityModuleSeeder(ILogger<IdentityModuleSeeder> logger, ...)
{
    public AtlasModule        GetModule()            => AtlasModules.Identity;
    public IModulePermissions GetModulePermissions() => new IdentityModulePermissions();
    public IModuleEntityTypes GetModuleEntityTypes() => new IdentityModuleEntityTypes();

    public async Task SeedAsync(CancellationToken ct = default)
    {
        logger.LogInformation("IdentityModuleSeeder started");
        await SeedRolesAsync(ct);
        await SeedInvitationsAsync(ct);
    }
}
```

## Partial File Pattern

```
Atlas.{Module}.Infrastructure/Seeders/
├── {Module}ModuleSeeder.cs               ← contract + SeedAsync (table of contents only)
├── {Module}ModuleSeeder.{Aggregate1}.cs  ← private Seed{Aggregate1}Async
└── {Module}ModuleSeeder.{Aggregate2}.cs  ← private Seed{Aggregate2}Async
```

## Seed Step Pattern

```csharp
// IdentityModuleSeeder.Roles.cs
public sealed partial class IdentityModuleSeeder
{
    private async Task SeedRolesAsync(CancellationToken ct)
    {
        if (await db.Roles.AnyAsync(ct))
        {
            logger.LogInformation("IdentityRoleSeeder skipped - data already exists");
            return;
        }

        logger.LogInformation("IdentityRoleSeeder started");

        // create domain objects, add to db
        setter.Set(tenantId, tenantName, SystemIdentity.UserId, SystemIdentity.Email);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("IdentityRoleSeeder completed — roles: root, admin, member");
    }
}
```

## Cross-Tenant Reads

```csharp
// ✅ bypass global multi-tenant filter — required in seeders
var exists = await db.Users
    .IgnoreQueryFilters()
    .AnyAsync(u => u.Id == BootstrapIdentity.RootUser.Id, ct);
```

## Naming

| Artifact | Pattern |
|---|---|
| Main file | `{Module}ModuleSeeder.cs` |
| Partial file | `{Module}ModuleSeeder.{Aggregate}.cs` |
| Seed method | `Seed{Aggregate}Async` |
