namespace Atlas.Identity.API.Endpoints.Roles.ListRoles;

public sealed class ListRolesRequest
{
    public bool IncludeInactive { get; init; } = false;
}
