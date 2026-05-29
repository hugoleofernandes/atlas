using Atlas.Identity.Application.Commands.UpdateRole;

namespace Atlas.Identity.API.Endpoints.Tenants._Roles.UpdateRole;

public sealed record UpdateRoleResponse(
    Guid                  RoleId,
    string                Name,
    IReadOnlyList<string> PermissionCodes
)
{
    public static UpdateRoleResponse From(UpdateRoleOutput output)
        => new(output.RoleId, output.Name, output.PermissionCodes);
}
