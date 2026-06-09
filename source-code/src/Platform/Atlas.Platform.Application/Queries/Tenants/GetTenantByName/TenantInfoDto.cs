namespace Atlas.Platform.Application.Queries.Tenants.GetTenantByName;

public sealed record TenantInfoDto(Guid TenantId, string TenantName, bool IsActive);
