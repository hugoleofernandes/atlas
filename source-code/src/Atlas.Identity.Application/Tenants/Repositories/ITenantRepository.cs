using Atlas.Identity.Domain.Entities.Tenants;

namespace Atlas.Identity.Application.Tenants.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetByNameWithUsersInvitationsAndRolesAsync(string name, CancellationToken ct);
}
