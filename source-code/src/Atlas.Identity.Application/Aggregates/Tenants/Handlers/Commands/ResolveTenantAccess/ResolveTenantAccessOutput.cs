namespace Atlas.Identity.Application.Aggregates.Tenants.Handlers.Commands.ResolveTenantAccess;

public sealed record ResolveTenantAccessOutput(
    Guid TenantId,
    string TenantName,
    Guid UserId,
    Guid RoleId,
    string RoleName,
    IReadOnlyList<string> Permissions
);
