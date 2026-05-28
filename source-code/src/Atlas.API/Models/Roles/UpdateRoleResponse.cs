using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.Identity.Application.Tenants.Roles.Handlers.Commands.UpdateRole;

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
