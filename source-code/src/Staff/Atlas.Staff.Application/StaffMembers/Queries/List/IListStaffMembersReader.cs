namespace Atlas.Staff.Application.StaffMembers.Queries.List;

public interface IListStaffMembersReader
{
    Task<IReadOnlyList<ListStaffMembersDto>> ListAsync(Guid tenantId, CancellationToken ct);
}
