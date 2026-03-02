using Atlas.BuildingBlocks.CQRS.Abstractions;
using Atlas.SharedKernel.Application;

namespace Atlas.Staff.Application.StaffMembers.Queries.List;

public sealed record Query(
    int Page,
    int PageSize
) : IQuery<PagedResult<Dto>>;