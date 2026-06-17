using Atlas.Platform.Application.Queries.Geography;
using Atlas.Platform.Application.Queries.Geography.GetCitiesByState;
using Atlas.Platform.Application.Queries.Geography.GetStatesByCountry;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Platform.Infrastructure.Readers.Geography;

public sealed class GeographyReader(PlatformDbContext db) : IGeographyReader
{
    private const string StatesSql = """
        SELECT
            s.id           AS StateId,
            s.country_code AS CountryCode,
            s.code         AS Code,
            s.name         AS Name
        FROM atlas_platform.states s
        WHERE s.is_active = true
        ORDER BY s.name ASC
        """;

    private const string CitiesSql = """
        SELECT
            c.id           AS CityId,
            s.country_code AS CountryCode,
            s.code         AS StateCode,
            c.name         AS Name
        FROM atlas_platform.cities c
        JOIN atlas_platform.states s ON s.id = c.state_id
        WHERE c.is_active = true
        ORDER BY c.name ASC
        """;

    public async Task<IReadOnlyList<StateDto>> LoadAllStatesAsync(CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var rows = await conn.QueryAsync<StateDto>(StatesSql);
        return rows.ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<CityDto>> LoadAllCitiesAsync(CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var rows = await conn.QueryAsync<CityDto>(CitiesSql);
        return rows.ToList().AsReadOnly();
    }
}
