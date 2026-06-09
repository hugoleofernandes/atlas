using Atlas.SharedKernel.Application;

namespace Atlas.Staff.Application.StaffMembers.Queries.List;

public interface IListStaffMembersReader
{
    Task<PagedResult<Dto>> ListAsync(
        int page,
        int pageSize,
        CancellationToken ct);
}