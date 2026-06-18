using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Queries.Lookups.LookupClassificationTypes;

public interface ILookupClassificationTypesQueryHandler
    : IQueryHandler<LookupClassificationTypesQuery, IReadOnlyList<ClassificationTypeLookupDto>>;
