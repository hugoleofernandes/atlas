using Atlas.Staff.Application.StaffMemberApp.Persistence;
using Atlas.Staff.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Staff.Infrastructure.Persistence.StaffMembers;

public sealed class StaffMemberRepository : IStaffMemberRepository
{
    private readonly StaffDbContext _db;

    public StaffMemberRepository(StaffDbContext db)
    {
        _db = db;
    }

    public Task<bool> ExistsAsync(
        Guid tenantId,
        Guid identityUserId,
        CancellationToken ct)
    {
        return _db.Set<StaffMember>()
            .AnyAsync(x =>
                x.TenantId == tenantId &&
                x.IdentityUserId == identityUserId,
                ct);
    }

    public async Task AddAsync(
        StaffMember staff,
        CancellationToken ct)
    {
        await _db.Set<StaffMember>().AddAsync(staff, ct);
    }
}