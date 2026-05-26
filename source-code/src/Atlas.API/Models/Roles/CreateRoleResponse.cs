using Atlas.API.Models;
using Atlas.Identity.Application.Tenants.Commands.CreateRole;

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
