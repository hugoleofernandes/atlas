using Atlas.Identity.Domain.Tenants;

namespace Atlas.Identity.Application.Tenants.Abstractions;

public interface ITenantRepository
{
    Task<Tenant?> GetBySlugWithUsersAndInvitationsAsync(string slug, CancellationToken ct);
}