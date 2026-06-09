using Atlas.Identity.Application.Commands.UpdateRole;

namespace Atlas.Identity.BffApi.Endpoints.Roles.UpdateRole;

public sealed record UpdateRoleResponse(
    Guid                  RoleId,
    string                Name,
    bool                  IsActive,
    IReadOnlyList<string> PermissionCodes
)
{
    public static UpdateRoleResponse From(UpdateRoleOutput output)
        => new(output.RoleId, output.Name, output.IsActive, output.PermissionCodes);
}
