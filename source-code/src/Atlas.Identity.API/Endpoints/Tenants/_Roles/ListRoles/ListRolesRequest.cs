namespace Atlas.Identity.API.Endpoints.Tenants._Roles.ListRoles;

public sealed class ListRolesRequest
{
    public int  Page            { get; init; } = 1;
    public int  PageSize        { get; init; } = 20;
    public bool IncludeInactive { get; init; } = false;
}
