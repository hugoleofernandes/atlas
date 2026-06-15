# Localization & Resources

## Rules

✅ Two resource types: error messages (`{Aggregate}Errors.resx`) and permission labels (`{Module}PermissionLabels.resx`)
✅ Every `.resx` file has a paired empty marker class in the **same namespace and folder**
✅ Both `.resx` (EN) and `.pt.resx` (PT) must contain the same set of keys
✅ Register every new `{Aggregate}Errors` localizer in `ErrorMessageLocalizer` in `Atlas.API`
✅ Error key format: `{aggregate}.{error_slug}` — must match the `ErrorCode` constant in the `DomainException`
✅ Permission label key = exact permission code string (e.g. `"identity.roles.read"`)
❌ Never mismatch namespace and folder path — resource resolution breaks silently with no error
❌ Never add a new resource file without registering its `IStringLocalizer<T>` in `ErrorMessageLocalizer`
❌ Never have keys in EN that are missing from PT (or vice versa) — `PermissionCatalogTranslationTests` fails on CI

## Folder Structure

```
Atlas.{Module}.Resources/
├── {Aggregate}/
│   ├── {Aggregate}Errors.cs          ← empty marker class
│   ├── {Aggregate}Errors.resx        ← EN error messages
│   └── {Aggregate}Errors.pt.resx    ← PT error messages
└── Permissions/
    ├── {Module}PermissionLabels.cs
    ├── {Module}PermissionLabels.resx
    └── {Module}PermissionLabels.pt.resx
```

## Marker Class Pattern

```csharp
// Atlas.Identity.Resources/Invitations/InvitationErrors.cs
namespace Atlas.Identity.Resources.Invitations;  // ← must match folder path

/// <summary>Marker class for IStringLocalizer&lt;InvitationErrors&gt;. Keys: invitation.*</summary>
public sealed class InvitationErrors { }
```

## Resx Key Pattern

```xml
<!-- InvitationErrors.resx -->
<data name="invitation.duplicate"><value>An active invitation for this email already exists.</value></data>
<data name="invitation.expired"  ><value>This invitation has expired.</value></data>

<!-- InvitationErrors.pt.resx — same keys, PT values -->
<data name="invitation.duplicate"><value>Já existe um convite ativo para este email.</value></data>
<data name="invitation.expired"  ><value>Este convite expirou.</value></data>
```

## ErrorMessageLocalizer Registration

When adding a new `{Aggregate}Errors` file, inject and register the localizer in `Atlas.API/Errors/ErrorMessageLocalizer.cs`:

```csharp
public ErrorMessageLocalizer(
    ...
    IStringLocalizer<InvitationErrors> invitation,   // ← add here
    IStringLocalizer<StaffMemberErrors> staff)
{
    _localizers = [..., invitation, staff];           // ← and here
}
```

If omitted, errors from that aggregate always fall back to the English `FallbackMessage` regardless of culture.

## How to Add a New Error Resource File

1. Create marker class in `Atlas.{Module}.Resources/{Aggregate}/{Aggregate}Errors.cs`
2. Create `{Aggregate}Errors.resx` and `{Aggregate}Errors.pt.resx` in the same folder
3. Add the same keys to both files
4. Inject `IStringLocalizer<{Aggregate}Errors>` into `ErrorMessageLocalizer` and add to `_localizers`
5. Use the key as `ErrorCode` in the `DomainException` subclass
