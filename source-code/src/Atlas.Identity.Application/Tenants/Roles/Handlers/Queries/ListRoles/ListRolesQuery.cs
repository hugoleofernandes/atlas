namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Queries.ListRoles;

public sealed record ListRolesQuery(int Page, int PageSize, bool IncludeInactive = false);
