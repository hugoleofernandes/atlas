namespace Atlas.Platform.Application.Queries.Geography.GetStatesByCountry;

public interface IGetStatesByCountryCache
{
    Task<IReadOnlyList<StateDto>> GetAsync(string countryCode, CancellationToken ct);
    void Invalidate();
}
