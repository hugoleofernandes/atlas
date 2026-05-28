namespace Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Queries.ListRoles;

public sealed record ListRolesQuery(int Page, int PageSize, bool IncludeInactive = false);
