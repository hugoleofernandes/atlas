namespace Atlas.Staff.Application.StaffMembers.Queries.List;

public sealed record Query(
    int Page,
    int PageSize
);

//public sealed record Query(
//    int Page,
//    int PageSize
//) : IQuery<PagedResult<Dto>>;