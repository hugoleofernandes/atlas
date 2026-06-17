using Atlas.Platform.Application.Queries.Geography.GetCitiesByState;

namespace Atlas.Platform.BffApi.Endpoints.Geography.GetCitiesByState;

public sealed record GetCitiesByStateResponse(Guid CityId, string Name)
{
    public static GetCitiesByStateResponse From(CityDto dto)
        => new(dto.CityId, dto.Name);
}
