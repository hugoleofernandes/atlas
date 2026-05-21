namespace Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;

public sealed record ResolveTenantAccessOutput(
    Guid TenantId,
    string TenantName,
    Guid UserId,
    Guid RoleId,
    string RoleName,
    IReadOnlyList<string> Permissions
);
