# Entity Types

## Rules

✅ Declare entity types in `{Module}ModuleEntityTypes` partial classes in `Atlas.{Module}.Contracts/EntityTypes/`
✅ One partial per aggregate: `{Module}ModuleEntityTypes.{Aggregate}.cs`
✅ Use `AtlasEntityType.Create(entitySuffix, name, module)` — suffix is a sequential number string ("1", "2", ...)
✅ Register every `AtlasEntityType` in `AllDefinitions` in the main `{Module}ModuleEntityTypes.cs`
✅ IDs are deterministic — same suffix + same module always produces the same GUID
✅ When creating a new auditable aggregate, create its `EntityType` in `Contracts` in the same change
✅ When introducing a module to the platform registry, ensure `AtlasBootstrapSeeder` includes that module's `GetModuleEntityTypes()`
✅ When introducing a module to the platform registry, ensure `AtlasBootstrapSeeder` includes that module's `GetModule()`, `GetModulePermissions()`, and `GetModuleEntityTypes()`
✅ Audit opt-in is owned by EF mapping via `Audit:EntityTypeId` annotation — not by a domain property
✅ Audited aggregate roots should inherit from `AuditedAggregateConfiguration<TEntity>` in Infrastructure so the annotation is applied centrally
✅ In TPH hierarchies, place the audit annotation on each concrete auditable subtype mapping (`Person`, `Organization`, etc.)
✅ For non-TPH aggregates, place the audit annotation in the aggregate's concrete EF configuration
✅ Whenever you create a new `*Configuration`, decide explicitly whether the mapped type is an auditable aggregate root or a non-audited/supporting type
✅ New aggregate-root configurations must choose one of two paths up front: `AuditedAggregateConfiguration<TEntity>` or plain `IEntityTypeConfiguration<T>`
❌ Never reuse a suffix within the same module — IDs would collide
❌ Never manually insert entity types in DB — `PlatformModuleSeeder` owns this table
❌ Never remove an entity type suffix and reuse it for a different aggregate — breaks audit history
❌ Never add an auditable aggregate without declaring and registering its `EntityType`
❌ Never assume `entity_types` sync alone is enough — the corresponding module must already exist in `atlas_platform.modules`
❌ Never push audit entity-type responsibility back into the domain model just because the aggregate uses TPH
❌ Never implement a dedicated domain audit-trail marker just to opt an aggregate into audit history
❌ Never implement an audited aggregate mapping directly from `IEntityTypeConfiguration<T>` when `AuditedAggregateConfiguration<TEntity>` fits the case
❌ Never create a new aggregate-root configuration without making an explicit audit decision

## Purpose

Entity types are the system's registry of auditable aggregates. Each aggregate root that participates in the audit trail must have an entity type declared. They are stored in Platform's DB (`atlas_platform.entity_types`) and referenced by audit log entries.

## How They Reach the Database

Each module declares its entity types via `IModuleEntityTypes.Definitions`.
At startup, `AtlasBootstrapSeeder` collects all modules' definitions and passes them to `PlatformModuleSeeder`, which syncs them into `atlas_platform.entity_types`:

- **New ID** → row created (active)
- **Existing ID** → reactivated if previously deactivated
- **ID removed from contracts** → marked inactive (never hard-deleted — audit logs may still reference it)

Declaring a new entity type in `AllDefinitions` is enough — the next startup syncs it to DB automatically.

If the module is not yet part of the bootstrap composition, also update `AtlasBootstrapSeeder` so its:
- `GetModule()`
- `GetModulePermissions()`
- `GetModuleEntityTypes()`

are included in the global sync inputs.

## Partial File Pattern

```csharp
// Atlas.{Module}.Contracts/EntityTypes/Partials/{Module}ModuleEntityTypes.{Aggregate}.cs
public sealed partial class IdentityModuleEntityTypes
{
    public static class Users
    {
        public static readonly AtlasEntityType EntityType =
            AtlasEntityType.Create("1", "User", AtlasModules.Identity);
    }

    public static class Roles
    {
        public static readonly AtlasEntityType EntityType =
            AtlasEntityType.Create("2", "Role", AtlasModules.Identity);
    }
}
```

## Audited EF Mapping Pattern

```csharp
public sealed class RoleConfiguration : AuditedAggregateConfiguration<Role>
{
    protected override Guid EntityTypeId => IdentityModuleEntityTypes.Roles.EntityType.Id;

    protected override void ConfigureEntity(EntityTypeBuilder<Role> b)
    {
        b.ToTable("roles");
        b.HasKey(x => x.Id);
    }
}
```

The base class owns the `Audit:EntityTypeId` annotation. The concrete mapping still owns table, keys, owned collections, and all other persistence details.

Practical rule:
- Aggregate root + auditable → declare an `EntityType` and inherit `AuditedAggregateConfiguration<TEntity>`
- Aggregate root + not audited → use `IEntityTypeConfiguration<T>` directly and leave it out of `EntityTypes`
- Non-aggregate/supporting type → use `IEntityTypeConfiguration<T>` directly

## Main File — Register in AllDefinitions

```csharp
// Atlas.{Module}.Contracts/EntityTypes/{Module}ModuleEntityTypes.cs
public sealed partial class IdentityModuleEntityTypes : IModuleEntityTypes
{
    public Guid ModuleId   => AtlasModules.Identity.Id;
    public string ModuleName => AtlasModules.Identity.Name;

    private static readonly IReadOnlyList<AtlasEntityType> AllDefinitions =
    [
        Users.EntityType,
        Roles.EntityType,
        Invitations.EntityType,
    ];

    public IReadOnlyList<AtlasEntityType> Definitions => AllDefinitions;
}
```

## ID Generation

`AtlasEntityType.Create(suffix, name, module)` produces a deterministic GUID:

```
00000000-0000-0000-{module.Code:D4}-{suffix.PadLeft(12, '0')}
```

Examples — Identity (code = 1):
- suffix "1" → `00000000-0000-0000-0001-000000000001`
- suffix "2" → `00000000-0000-0000-0001-000000000002`

Suffixes are scoped per module — "1" in Identity and "1" in Staff produce different GUIDs.
