using Atlas.Party.Application.Repositories;
using Atlas.Party.Domain.Parties;
using Atlas.Party.Domain.Shared;
using Atlas.Party.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Party.Infrastructure.Repositories;

public sealed class PersonRepository : IPersonRepository
{
    private readonly PartyDbContext _db;

    public PersonRepository(PartyDbContext db)
    {
        _db = db;
    }

    public async Task<Person?> GetByIdAsync(Guid partyId, CancellationToken ct)
    {
        return await _db.Persons.FirstOrDefaultAsync(i => i.Id == partyId, ct);
    }

    public async Task<bool> ExistsWithTaxNumberAsync(Guid tenantId, string taxNumber, CancellationToken ct)
    {
        var parsed = TaxNumber.Create(taxNumber);
        return await _db.Parties.AnyAsync(p => p.TenantId == tenantId && p.TaxNumber == parsed, ct);
    }

    public async Task AddAsync(Person person, CancellationToken ct)
    {
        await _db.Persons.AddAsync(person, ct);
    }

    public void Remove(Person person)
    {
        _db.Persons.Remove(person);
    }
}

