namespace Atlas.Identity.Domain.Tenants._Roles._Permissions;

/// <summary>
/// System-level permission that bypasses all authorization checks.
/// Not assignable to custom roles — only added to AllIncludingSystem by PermissionPolicyService.
/// </summary>
public static class SystemPermissions
{
    public const string Root = "system.root";
}
