using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Queries.Lookups.LookupContactTypes;

public interface ILookupContactTypesQueryHandler
    : IQueryHandler<LookupContactTypesQuery, IReadOnlyList<ContactTypeLookupDto>>;
