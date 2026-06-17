using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Platform.Application.Queries.Geography.GetCitiesByState;

public interface IGetCitiesByStateQueryHandler
    : IQueryHandler<GetCitiesByStateQuery, IReadOnlyList<CityDto>>;
