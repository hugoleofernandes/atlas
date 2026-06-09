namespace Atlas.Identity.Application.Commands.DevLogin;

public sealed record DevLoginOutput(
    Guid TenantId,
    string TenantName,
    Guid UserId,
    Guid RoleId,
    string RoleName,
    IReadOnlyList<string> Permissions
);
