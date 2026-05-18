namespace Atlas.Identity.Application.Tenants.Commands.CreateRole;

public sealed record Command(
    string Name,
    IEnumerable<string> PermissionCodes
);
