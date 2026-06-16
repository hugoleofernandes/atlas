using Atlas.Party.Application.Repositories;
using Atlas.Party.Domain.Parties;
using Atlas.Party.Domain.Shared;
using Atlas.Party.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Party.Infrastructure.Repositories;

public sealed class IndividualRepository : IIndividualRepository
{
    private readonly PartyDbContext _db;

    public IndividualRepository(PartyDbContext db)
    {
        _db = db;
    }

    public async Task<Individual?> GetByIdAsync(Guid partyId, CancellationToken ct)
    {
        return await _db.Individuals.FirstOrDefaultAsync(i => i.Id == partyId, ct);
    }

    public async Task<bool> ExistsWithTaxNumberAsync(Guid tenantId, string taxNumber, CancellationToken ct)
    {
        var parsed = TaxNumber.Create(taxNumber);
        return await _db.Parties.AnyAsync(p => p.TenantId == tenantId && p.TaxNumber == parsed, ct);
    }

    public async Task AddAsync(Individual individual, CancellationToken ct)
    {
        await _db.Individuals.AddAsync(individual, ct);
    }

    public void Remove(Individual individual)
    {
        _db.Individuals.Remove(individual);
    }
}
