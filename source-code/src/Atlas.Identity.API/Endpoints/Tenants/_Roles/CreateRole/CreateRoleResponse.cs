using Atlas.Identity.Application.Commands.CreateRole;

namespace Atlas.Identity.API.Endpoints.Tenants._Roles.CreateRole;

public sealed record CreateRoleResponse(
    Guid                    RoleId,
    string                  Name,
    IReadOnlyList<string>   PermissionCodes
)
{
    public static CreateRoleResponse From(CreateRoleOutput output)
        => new(output.RoleId, output.Name, output.PermissionCodes);
}
