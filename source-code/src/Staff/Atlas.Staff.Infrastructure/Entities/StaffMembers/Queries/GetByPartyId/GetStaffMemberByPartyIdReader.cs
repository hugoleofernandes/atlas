using Atlas.Staff.Application.StaffMembers.Queries.GetByPartyId;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Staff.Infrastructure.Entities.StaffMembers.Queries.GetByPartyId;

public sealed class GetStaffMemberByPartyIdReader(StaffDbContext db) : IGetStaffMemberByPartyIdReader
{
    private const string Sql = """
        SELECT
            id     AS StaffMemberId,
            status AS Status
        FROM atlas_staff.staff_members
        WHERE party_id  = @PartyId
          AND tenant_id = @TenantId
        LIMIT 1
        """;

    public async Task<GetStaffMemberByPartyIdDto?> FindAsync(
        Guid partyId,
        Guid tenantId,
        CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        return await conn.QueryFirstOrDefaultAsync<GetStaffMemberByPartyIdDto>(
            Sql,
            new { PartyId = partyId, TenantId = tenantId });
    }
}
