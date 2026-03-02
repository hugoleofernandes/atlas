using Atlas.BuildingBlocks.CQRS.Abstractions;
using Atlas.SharedKernel.Application;

namespace Atlas.Staff.Application.StaffMembers.Queries.List;

public sealed class Handler
    : IQueryHandler<Query, PagedResult<Dto>>
{
    private readonly IListStaffMembersReader _reader;

    public Handler(IListStaffMembersReader reader)
    {
        _reader = reader;
    }

    public Task<PagedResult<Dto>> Handle(
        Query query,
        CancellationToken ct)
    {
        return _reader.ListAsync(
            query.Page,
            query.PageSize,
            ct);
    }
}