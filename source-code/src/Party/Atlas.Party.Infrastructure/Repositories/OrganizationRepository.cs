using Atlas.Party.Application.Repositories;
using Atlas.Party.Domain.Parties;
using Atlas.Party.Domain.Shared;
using Atlas.Party.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Party.Infrastructure.Repositories;

public sealed class OrganizationRepository : IOrganizationRepository
{
    private readonly PartyDbContext _db;

    public OrganizationRepository(PartyDbContext db)
    {
        _db = db;
    }

    public async Task<Organization?> GetByIdAsync(Guid partyId, CancellationToken ct)
    {
        return await _db.Organizations.FirstOrDefaultAsync(o => o.Id == partyId, ct);
    }

    public async Task<bool> ExistsWithTaxNumberAsync(Guid tenantId, string taxNumber, CancellationToken ct)
    {
        var parsed = TaxNumber.Create(taxNumber);
        return await _db.Parties.AnyAsync(p => p.TenantId == tenantId && p.TaxNumber == parsed, ct);
    }

    public async Task AddAsync(Organization organization, CancellationToken ct)
    {
        await _db.Organizations.AddAsync(organization, ct);
    }

    public void Remove(Organization organization)
    {
        _db.Organizations.Remove(organization);
    }
}
