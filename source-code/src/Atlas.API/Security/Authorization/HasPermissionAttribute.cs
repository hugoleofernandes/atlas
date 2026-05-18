using Microsoft.AspNetCore.Authorization;

namespace Atlas.API.Security.Authorization;

/// <summary>
/// Requires the authenticated user to hold the specified permission claim.
/// Works with PermissionAuthorizationHandler + PermissionPolicyProvider.
///
/// Usage:
///   [HasPermission(Permissions.Staff.Deactivate)]
///   public async Task&lt;IActionResult&gt; Deactivate(...)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "permission:";

    public HasPermissionAttribute(string permission)
        : base($"{PolicyPrefix}{permission}")
    {
    }
}
