namespace Atlas.SharedKernel.Application;

public sealed class PagedResult<T>
{
    public IEnumerable<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }

    public PagedResult(
        IEnumerable<T> items,
        int page,
        int pageSize,
        int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}