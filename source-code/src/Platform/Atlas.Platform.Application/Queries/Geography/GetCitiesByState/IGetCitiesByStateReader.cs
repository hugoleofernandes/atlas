namespace Atlas.Platform.Application.Queries.Geography.GetCitiesByState;

public interface IGetCitiesByStateReader
{
    Task<IReadOnlyList<CityDto>> ReadAsync(CancellationToken ct);
}
