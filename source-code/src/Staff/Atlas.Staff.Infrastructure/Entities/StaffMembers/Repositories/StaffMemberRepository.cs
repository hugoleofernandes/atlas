using Atlas.Staff.Application.StaffMemberApp.Persistence;
using Atlas.Staff.Domain.Entities;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Staff.Infrastructure.Entities.StaffMembers.Repositories;

public sealed class StaffMemberRepository(StaffDbContext db) : IStaffMemberRepository
{
    public Task<StaffMember?> GetByIdAsync(Guid staffMemberId, CancellationToken ct)
        => db.Set<StaffMember>().FirstOrDefaultAsync(x => x.Id == staffMemberId, ct);

    public Task<bool> ExistsAsync(Guid tenantId, Guid userId, CancellationToken ct)
        => db.Set<StaffMember>().AnyAsync(x => x.TenantId == tenantId && x.UserId == userId, ct);

    public Task<bool> ExistsForPartyAsync(Guid tenantId, Guid partyId, CancellationToken ct)
        => db.Set<StaffMember>().AnyAsync(x => x.TenantId == tenantId && x.PartyId == partyId, ct);

    public async Task AddAsync(StaffMember staffMember, CancellationToken ct)
        => await db.Set<StaffMember>().AddAsync(staffMember, ct);
}
