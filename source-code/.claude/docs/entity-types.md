# Entity Types

## Rules

✅ Declare entity types in `{Module}ModuleEntityTypes` partial classes in `Atlas.{Module}.Contracts/EntityTypes/`
✅ One partial per aggregate: `{Module}ModuleEntityTypes.{Aggregate}.cs`
✅ Use `AtlasEntityType.Create(entitySuffix, name, module)` — suffix is a sequential number string ("1", "2", ...)
✅ Register every `AtlasEntityType` in `AllDefinitions` in the main `{Module}ModuleEntityTypes.cs`
✅ IDs are deterministic — same suffix + same module always produces the same GUID
❌ Never reuse a suffix within the same module — IDs would collide
❌ Never manually insert entity types in DB — `PlatformModuleSeeder` owns this table
❌ Never remove an entity type suffix and reuse it for a different aggregate — breaks audit history

## Purpose

Entity types are the system's registry of auditable aggregates. Each aggregate root that participates in the audit trail must have an entity type declared. They are stored in Platform's DB (`atlas_platform.entity_types`) and referenced by audit log entries.

## How They Reach the Database

Each module declares its entity types via `IModuleEntityTypes.Definitions`.
At startup, `AtlasBootstrapSeeder` collects all modules' definitions and passes them to `PlatformModuleSeeder`, which syncs them into `atlas_platform.entity_types`:

- **New ID** → row created (active)
- **Existing ID** → reactivated if previously deactivated
- **ID removed from contracts** → marked inactive (never hard-deleted — audit logs may still reference it)

Declaring a new entity type in `AllDefinitions` is enough — the next startup syncs it to DB automatically.

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
