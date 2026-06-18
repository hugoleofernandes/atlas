namespace Atlas.Platform.Application.Queries.Geography.GetCitiesByState;

public interface IGetCitiesByStateCache
{
    Task<IReadOnlyList<CityDto>> GetAsync(string countryCode, string stateCode, CancellationToken ct);
    void Invalidate();
}
