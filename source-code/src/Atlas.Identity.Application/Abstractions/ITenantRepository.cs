using Atlas.Identity.Domain.Entities;

namespace Atlas.Identity.Application.Abstractions;

public interface ITenantRepository
{
    Task<Tenant?> GetBySlugWithMembershipsAsync(
        string slug,
        CancellationToken ct);
}