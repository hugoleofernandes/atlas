using Atlas.Platform.Application.Queries.Geography.GetCitiesByState;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Platform.Infrastructure.Readers.Geography.GetCitiesByState;

public sealed class GetCitiesByStateReader(PlatformDbContext db) : IGetCitiesByStateReader
{
    private const string Sql = """
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

    public async Task<IReadOnlyList<CityDto>> ReadAsync(CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var rows = await conn.QueryAsync<CityDto>(Sql);
        return rows.ToList().AsReadOnly();
    }
}
