using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Platform.Application.Queries.Geography.GetStatesByCountry;

public interface IGetStatesByCountryQueryHandler
    : IQueryHandler<GetStatesByCountryQuery, IReadOnlyList<StateDto>>;
