using Atlas.Identity.Domain.Entities.Tenants;

namespace Atlas.Identity.Application.Abstractions.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetByNameWithUsersAndInvitationsAsync(string name, CancellationToken ct);
}