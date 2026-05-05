using Atlas.Identity.Domain.Tenants;

namespace Atlas.Identity.Application.Tenants.Abstractions;

public interface ITenantRepository
{
    Task<Tenant?> GetBySlugWithMembershipsAsync(
        string slug,
        CancellationToken ct);
}