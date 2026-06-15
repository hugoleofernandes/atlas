# Permissions (RBAC)

## Rules

✅ Permission codes: `{module}.{resource}.{verb}` — enforced at runtime by `PermissionDefinition`
✅ Declare permissions in `{Module}ModulePermissions` partial classes in `Atlas.{Module}.Contracts/Permissions/`
✅ One partial file per resource: `{Module}ModulePermissions.{Resource}.cs`
✅ Register every new `PermissionDefinition` in `AllDefinitions` in the main `{Module}ModulePermissions.cs`
✅ Add translations to `{Module}PermissionLabels.resx` (EN) and `.pt.resx` (PT) — CI test will fail otherwise
✅ Endpoints use `Policies($"permission:{ModulePermissions.Resource.Verb}")` — always specific verb
✅ `manage` = `isManager: true` — assignment shortcut only (covers all verbs for the resource)
❌ Never use `manage` in `Policies()` on an endpoint — always use the specific verb
❌ Never add a permission to an endpoint without declaring it in `{Module}ModulePermissions` first
❌ `IPermissionPolicy` was removed — do not reference it

## Available Verbs

`read` | `create` | `update` | `delete` | `deactivate` (exceptional) | `manage` (assignment only)

## Partial File Pattern

```csharp
// Atlas.{Module}.Contracts/Permissions/Partials/{Module}ModulePermissions.{Resource}.cs
public sealed partial class StaffModulePermissions
{
    public static class StaffMember
    {
        public const string Read       = "staff.staff-member.read";
        public const string Create     = "staff.staff-member.create";
        public const string Update     = "staff.staff-member.update";
        public const string Deactivate = "staff.staff-member.deactivate";
        public const string Manage     = "staff.staff-member.manage";
    }
}
```

## Main File — Register in AllDefinitions

```csharp
// Atlas.{Module}.Contracts/Permissions/{Module}ModulePermissions.cs
public sealed partial class StaffModulePermissions : IModulePermissions
{
    private static readonly IReadOnlyList<PermissionDefinition> AllDefinitions =
    [
        new(StaffMember.Read,       false, AtlasModules.Staff),
        new(StaffMember.Create,     false, AtlasModules.Staff),
        new(StaffMember.Update,     false, AtlasModules.Staff),
        new(StaffMember.Deactivate, false, AtlasModules.Staff),
        new(StaffMember.Manage,     true,  AtlasModules.Staff),  // isManager: true
    ];

    public IReadOnlyList<PermissionDefinition> Definitions => AllDefinitions;
}
```

## Endpoint Usage

```csharp
// ✅ specific verb
Policies($"permission:{StaffModulePermissions.StaffMember.Read}");

// ❌ manage on an endpoint — never
Policies($"permission:{StaffModulePermissions.StaffMember.Manage}");
```

## How Permissions Reach the Database

Each module declares its permissions in code (`IModulePermissions.Definitions`).
At startup, `IdentityPermissionCatalogSeeder` collects all modules' definitions and syncs them to `atlas_identity.permissions`:

- **New code** → row created (active)
- **Existing code** → reactivated if previously deactivated
- **Code removed from code** → marked inactive (never hard-deleted — roles may still reference it)

This means: **declaring a permission in `AllDefinitions` is enough — the next startup syncs it to DB automatically.** No manual DB insert needed.

## How to Add a New Permission

1. Add constants to the resource partial (or create `{Module}ModulePermissions.{Resource}.cs`)
2. Register `new PermissionDefinition(...)` in `AllDefinitions` in the main file
3. Add translations to `{Module}PermissionLabels.resx` and `.pt.resx` (key = permission code)
4. `PermissionCatalogTranslationTests` validates both files — CI fails if a key is missing
5. On next startup, `IdentityPermissionCatalogSeeder` syncs the new code to `atlas_identity.permissions`
