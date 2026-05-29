namespace Atlas.Identity.Application.Queries.Roles.ListRoles;

public sealed record ListRolesQuery(int Page, int PageSize, bool IncludeInactive = false);
