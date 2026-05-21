namespace Atlas.Identity.Application.Tenants.Queries.ListRoles;

public sealed record ListRolesQuery(int Page, int PageSize, bool IncludeInactive = false);
