
using Atlas.Identity.Domain.Entities.Tenants;

namespace Atlas.Identity.Application.Tenants.Abstractions;

public interface ITenantRepository
{
    Task<Tenant?> GetByNameWithUsersAndInvitationsAsync(string name, CancellationToken ct);
}