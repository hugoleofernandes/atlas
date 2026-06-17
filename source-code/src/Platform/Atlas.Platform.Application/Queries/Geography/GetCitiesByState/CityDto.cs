namespace Atlas.Platform.Application.Queries.Geography.GetCitiesByState;

public sealed record CityDto(
    Guid   CityId,
    string CountryCode,
    string StateCode,
    string Name
);
