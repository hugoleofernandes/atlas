namespace Atlas.Identity.Application.Commands.ActivateRole;

public sealed record ActivateRoleOutput(Guid RoleId, bool IsActive);
