using Atlas.Staff.Application.StaffMemberApp.Persistence;
using Atlas.Staff.Domain.Entities;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Staff.Infrastructure.Entities.StaffMembers.Repositories;

public sealed class StaffMemberRepository : IStaffMemberRepository
{
    private readonly StaffDbContext _db;

    public StaffMemberRepository(StaffDbContext db)
    {
        _db = db;
    }

    public Task<bool> ExistsAsync(
        Guid tenantId,
        Guid UserId,
        CancellationToken ct)
    {
        return _db.Set<StaffMember>()
            .AnyAsync(x =>
                x.TenantId == tenantId &&
                x.UserId == UserId,
                ct);
    }

    public async Task AddAsync(
        StaffMember staff,
        CancellationToken ct)
    {
        await _db.Set<StaffMember>().AddAsync(staff, ct);
    }
}