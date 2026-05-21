namespace Atlas.BuildingBlocks.AspNetCore.Security;

public static class AtlasClaims
{
    public const string TenantId = "tenant_id";

    public const string TenantName = "tenant_name";

    public const string UserId = "user_id";

    public const string RoleId = "role_id";

    public const string UserEmail = "user_email";

    public const string Permission = "permission";

    public const string BootstrapCompleted = "atlas_bootstrap_completed";

    // Permission value that grants unrestricted access — bypasses all permission checks.
    public const string RootPermission = "system.root";
}
