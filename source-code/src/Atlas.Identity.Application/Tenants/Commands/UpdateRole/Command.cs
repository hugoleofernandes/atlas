namespace Atlas.Identity.Application.Tenants.Commands.UpdateRole;

public sealed record Command(Guid RoleId, string Name, IReadOnlyList<string> PermissionCodes);
