namespace Atlas.Platform.Application.Queries.Lookups.LookupStatuses;

public sealed class LookupStatusesQueryHandler(ILookupStatusesReader reader) : ILookupStatusesQueryHandler
{
    public Task<IReadOnlyList<StatusLookupDto>> ExecuteAsync(LookupStatusesQuery query, CancellationToken ct)
        => reader.LookupAsync(ct);
}
