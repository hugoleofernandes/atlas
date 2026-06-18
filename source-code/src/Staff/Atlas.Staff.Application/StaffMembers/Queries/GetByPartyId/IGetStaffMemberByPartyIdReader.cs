namespace Atlas.Staff.Application.StaffMembers.Queries.GetByPartyId;

public interface IGetStaffMemberByPartyIdReader
{
    Task<GetStaffMemberByPartyIdDto?> FindAsync(Guid partyId, Guid tenantId, CancellationToken ct);
}
