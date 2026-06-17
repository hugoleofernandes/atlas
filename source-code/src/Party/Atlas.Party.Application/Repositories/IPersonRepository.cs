using Atlas.Party.Domain.Parties;

namespace Atlas.Party.Application.Repositories;

public interface IPersonRepository
{
    /// <summary>Loads a person by ID. Returns null if not found.</summary>
    Task<Person?> GetByIdAsync(Guid partyId, CancellationToken ct);

    /// <summary>Returns true if a party with the given tax number already exists for the tenant.</summary>
    Task<bool> ExistsWithTaxNumberAsync(Guid tenantId, string taxNumber, CancellationToken ct);

    Task AddAsync(Person person, CancellationToken ct);

    void Remove(Person person);
}

