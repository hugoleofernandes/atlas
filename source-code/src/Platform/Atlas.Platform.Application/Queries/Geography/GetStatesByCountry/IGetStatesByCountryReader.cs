namespace Atlas.Platform.Application.Queries.Geography.GetStatesByCountry;

public interface IGetStatesByCountryReader
{
    Task<IReadOnlyList<StateDto>> ReadAsync(CancellationToken ct);
}
