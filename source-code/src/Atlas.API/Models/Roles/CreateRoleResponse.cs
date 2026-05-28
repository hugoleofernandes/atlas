using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.Identity.Application.Tenants.Roles.Handlers.Commands.CreateRole;

namespace Atlas.API.Models.Roles;

public sealed record CreateRoleResponse(
    Guid RoleId,
    string Name,
    IReadOnlyList<string> PermissionCodes
) : IResponseFrom<CreateRoleOutput, CreateRoleResponse>
{
    public static CreateRoleResponse From(CreateRoleOutput output)
        => new(output.RoleId, output.Name, output.PermissionCodes);
}
