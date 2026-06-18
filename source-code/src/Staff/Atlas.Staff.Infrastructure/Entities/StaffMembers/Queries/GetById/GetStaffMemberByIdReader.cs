using Atlas.Staff.Application.StaffMembers.Queries.GetById;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Staff.Infrastructure.Entities.StaffMembers.Queries.GetById;

public sealed class GetStaffMemberByIdReader(StaffDbContext db) : IGetStaffMemberByIdReader
{
    private const string Sql = """
        SELECT
            id                AS StaffMemberId,
            tenant_id         AS TenantId,
            party_id          AS PartyId,
            employee_number   AS EmployeeNumber,
            contract_type     AS ContractType,
            hire_date         AS HireDate,
            termination_date  AS TerminationDate,
            status            AS Status,
            created_at        AS CreatedAt
        FROM atlas_staff.staff_members
        WHERE id = @StaffMemberId
          AND tenant_id = @TenantId
        """;

    public async Task<GetStaffMemberByIdDto?> FindAsync(
        Guid staffMemberId,
        Guid tenantId,
        CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        return await conn.QueryFirstOrDefaultAsync<GetStaffMemberByIdDto>(
            Sql,
            new { StaffMemberId = staffMemberId, TenantId = tenantId });
    }
}
