namespace Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;

public sealed record Output(
    Guid TenantId,
    string TenantName,
    Guid UserId,
    Guid TenantRoleId,
    string RoleName,
    IReadOnlyList<string> Permissions
);
