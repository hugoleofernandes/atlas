using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Queries.Lookups.LookupGenders;

public interface ILookupGendersQueryHandler
    : IQueryHandler<LookupGendersQuery, IReadOnlyList<GenderLookupDto>>;
