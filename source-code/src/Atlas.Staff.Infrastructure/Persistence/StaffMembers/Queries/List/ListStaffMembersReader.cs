using Atlas.Staff.Application.StaffMembers.Queries.List;
using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Staff.Infrastructure.Persistence.StaffMembers.Queries.List;

public sealed class ListStaffMembersReader
    : IListStaffMembersReader
{
    private readonly StaffDbContext _db;

    public ListStaffMembersReader(StaffDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<Dto>> ListAsync(
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var baseQuery = _db.StaffMembers
            .AsNoTracking();

        var total = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderBy(x => x.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new Dto(
                x.Id,
                x.FirstName,
                x.LastName,
                x.Role,
                x.IsActive))
            .ToListAsync(ct);

        return new PagedResult<Dto>(
            items,
            page,
            pageSize,
            total);
    }
}