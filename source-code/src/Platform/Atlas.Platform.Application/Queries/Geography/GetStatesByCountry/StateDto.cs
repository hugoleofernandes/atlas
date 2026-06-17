namespace Atlas.Platform.Application.Queries.Geography.GetStatesByCountry;

public sealed record StateDto(
    Guid   StateId,
    string CountryCode,
    string Code,
    string Name
);
