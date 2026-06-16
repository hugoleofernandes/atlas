using Atlas.Party.Domain.Parties;

namespace Atlas.Party.Application.Repositories;

public interface IOrganizationRepository
{
    /// <summary>Loads an organization by ID. Returns null if not found.</summary>
    Task<Organization?> GetByIdAsync(Guid partyId, CancellationToken ct);

    /// <summary>Returns true if a party with the given tax number already exists for the tenant.</summary>
    Task<bool> ExistsWithTaxNumberAsync(Guid tenantId, string taxNumber, CancellationToken ct);

    Task AddAsync(Organization organization, CancellationToken ct);

    void Remove(Organization organization);
}
