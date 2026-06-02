namespace Atlas.Identity.Application.Commands.DeactivateRole;

public sealed record DeactivateRoleOutput(Guid RoleId, bool IsActive);
