using Atlas.Platform.Application.Queries.Geography.GetStatesByCountry;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Platform.Infrastructure.Readers.Geography.GetStatesByCountry;

public sealed class GetStatesByCountryReader(PlatformDbContext db) : IGetStatesByCountryReader
{
    private const string Sql = """
        SELECT
            s.id           AS StateId,
            s.country_code AS CountryCode,
            s.code         AS Code,
            s.name         AS Name
        FROM atlas_platform.states s
        WHERE s.is_active = true
        ORDER BY s.name ASC
        """;

    public async Task<IReadOnlyList<StateDto>> ReadAsync(CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var rows = await conn.QueryAsync<StateDto>(Sql);
        return rows.ToList().AsReadOnly();
    }
}
