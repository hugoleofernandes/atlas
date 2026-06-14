namespace Atlas.Platform.Application.Queries.Tenants.GetTenantsByIds;

public sealed record TenantLookupDto(Guid TenantId, string TenantName, bool IsActive);
