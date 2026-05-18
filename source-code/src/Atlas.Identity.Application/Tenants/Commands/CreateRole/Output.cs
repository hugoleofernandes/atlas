namespace Atlas.Identity.Application.Tenants.Commands.CreateRole;

public sealed record Output(
    Guid RoleId,
    string Name,
    IReadOnlyList<string> PermissionCodes
);
