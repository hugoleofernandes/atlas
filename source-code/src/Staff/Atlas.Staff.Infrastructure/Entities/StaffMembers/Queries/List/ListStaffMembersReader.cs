using Atlas.Staff.Application.StaffMembers.Queries.List;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Staff.Infrastructure.Entities.StaffMembers.Queries.List;

public sealed class ListStaffMembersReader(StaffDbContext db) : IListStaffMembersReader
{
    private const string Sql = """
        SELECT
            id               AS StaffMemberId,
            party_id         AS PartyId,
            employee_number  AS EmployeeNumber,
            contract_type    AS ContractType,
            status           AS Status,
            hire_date        AS HireDate
        FROM atlas_staff.staff_members
        WHERE tenant_id = @TenantId
        ORDER BY created_at DESC
        """;

    public async Task<IReadOnlyList<ListStaffMembersDto>> ListAsync(Guid tenantId, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var rows = await conn.QueryAsync<ListStaffMembersDto>(Sql, new { TenantId = tenantId });
        return rows.ToList();
    }
}
