namespace Atlas.Platform.Application.Queries.Lookups.LookupStatuses;

public interface ILookupStatusesReader
{
    Task<IReadOnlyList<StatusLookupDto>> LookupAsync(CancellationToken ct);
}
