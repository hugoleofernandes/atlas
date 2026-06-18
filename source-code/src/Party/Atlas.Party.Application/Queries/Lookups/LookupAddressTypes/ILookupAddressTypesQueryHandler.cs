using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Queries.Lookups.LookupAddressTypes;

public interface ILookupAddressTypesQueryHandler
    : IQueryHandler<LookupAddressTypesQuery, IReadOnlyList<AddressTypeLookupDto>>;
