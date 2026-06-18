using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Platform.Application.Queries.Lookups.LookupStatuses;

public interface ILookupStatusesQueryHandler : IQueryHandler<LookupStatusesQuery, IReadOnlyList<StatusLookupDto>>;
