namespace Atlas.Staff.Application.StaffMembers.Queries.GetById;

public interface IGetStaffMemberByIdReader
{
    Task<GetStaffMemberByIdDto?> FindAsync(Guid staffMemberId, Guid tenantId, CancellationToken ct);
}
