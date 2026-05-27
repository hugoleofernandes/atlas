namespace Atlas.Identity.Application.Tenants.Queries.Dtos;

public sealed record PermissionGroupDto(string Manage, IReadOnlyList<string> Granular);
