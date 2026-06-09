using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Platform.Application.Queries.EntityTypes.Lookup;

public interface ILookupEntityTypesQueryHandler
    : IQueryHandler<LookupEntityTypesQuery, IReadOnlyList<EntityTypeLookupDto>>;

