using Atlas.API.Models;
using Atlas.Identity.Application.Tenants.Commands.UpdateRole;

namespace Atlas.API.Models.Roles;

public sealed record UpdateRoleResponse(
    Guid RoleId,
    string Name,
    IReadOnlyList<string> PermissionCodes
) : IResponseFrom<UpdateRoleOutput, UpdateRoleResponse>
{
    public static UpdateRoleResponse From(UpdateRoleOutput output)
        => new(output.RoleId, output.Name, output.PermissionCodes);
}
