using Atlas.Identity.Application.Commands.CreateRole;

namespace Atlas.Identity.BffApi.Endpoints.Roles.CreateRole;

public sealed record CreateRoleResponse(
    Guid                    RoleId,
    string                  Name,
    bool                    IsActive,
    IReadOnlyList<string>   PermissionCodes
)
{
    public static CreateRoleResponse From(CreateRoleOutput output)
        => new(output.RoleId, output.Name, output.IsActive, output.PermissionCodes);
}
