using Atlas.SharedKernel.Application;
using Atlas.Staff.Application.StaffMembers.Queries.List;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Staff.Infrastructure.Entities.StaffMembers.Queries.List;

public sealed class ListStaffMembersReader(StaffDbContext db, IRequestContext context)
    : IListStaffMembersReader
{
    private const string CountSql = """
        SELECT COUNT(*)
        FROM atlas_staff.staff_members
        WHERE tenant_id = @TenantId
        """;

    private const string PageSql = """
        SELECT id, first_name AS FirstName, last_name AS LastName, role AS Role, is_active AS IsActive
        FROM atlas_staff.staff_members
        WHERE tenant_id = @TenantId
        ORDER BY first_name ASC
        LIMIT @PageSize OFFSET @Offset
        """;

    public async Task<PagedResult<Dto>> ListAsync(int page, int pageSize, CancellationToken ct)
    {
        var tenantId = context.TenantId!.Value;
        var conn     = db.Database.GetDbConnection();
        var param    = new { TenantId = tenantId, PageSize = pageSize, Offset = (page - 1) * pageSize };

        var total = await conn.ExecuteScalarAsync<int>(CountSql, new { TenantId = tenantId });
        var items = (await conn.QueryAsync<Dto>(PageSql, param)).ToList();

        return new PagedResult<Dto>(
            items:      items,
            page:       page,
            pageSize:   pageSize,
            totalCount: total);
    }
}
