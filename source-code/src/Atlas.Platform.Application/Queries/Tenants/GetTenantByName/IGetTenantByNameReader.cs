namespace Atlas.Platform.Application.Queries.Tenants.GetTenantByName;

public interface IGetTenantByNameReader
{
    Task<TenantInfoDto?> ReadAsync(string tenantName, CancellationToken ct);
}
