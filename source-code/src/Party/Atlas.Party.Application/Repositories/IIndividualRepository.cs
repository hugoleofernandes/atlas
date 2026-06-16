using Atlas.Party.Domain.Parties;

namespace Atlas.Party.Application.Repositories;

public interface IIndividualRepository
{
    /// <summary>Loads an individual by ID. Returns null if not found.</summary>
    Task<Individual?> GetByIdAsync(Guid partyId, CancellationToken ct);

    /// <summary>Returns true if a party with the given tax number already exists for the tenant.</summary>
    Task<bool> ExistsWithTaxNumberAsync(Guid tenantId, string taxNumber, CancellationToken ct);

    Task AddAsync(Individual individual, CancellationToken ct);

    void Remove(Individual individual);
}
