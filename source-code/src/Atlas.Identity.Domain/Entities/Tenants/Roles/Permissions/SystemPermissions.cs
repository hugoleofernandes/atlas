namespace Atlas.Identity.Domain.Entities.Tenants.Roles.Permissions;

/// <summary>
/// System-level permission that bypasses all authorization checks.
/// Not assignable to custom roles — only added to AllIncludingSystem by PermissionPolicyService.
/// </summary>
public static class SystemPermissions
{
    public const string Root = "system.root";
}
