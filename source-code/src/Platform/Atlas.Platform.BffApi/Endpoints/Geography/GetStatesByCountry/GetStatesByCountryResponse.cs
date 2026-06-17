using Atlas.Platform.Application.Queries.Geography.GetStatesByCountry;

namespace Atlas.Platform.BffApi.Endpoints.Geography.GetStatesByCountry;

public sealed record GetStatesByCountryResponse(Guid StateId, string Code, string Name)
{
    public static GetStatesByCountryResponse From(StateDto dto)
        => new(dto.StateId, dto.Code, dto.Name);
}
