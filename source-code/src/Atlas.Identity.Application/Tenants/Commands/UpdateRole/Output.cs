namespace Atlas.Identity.Application.Tenants.Commands.UpdateRole;

public sealed record Output(Guid RoleId, string Name, IReadOnlyList<string> PermissionCodes);
