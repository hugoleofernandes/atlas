using Atlas.Party.Application.Queries.Lookups.LookupGenders;
using Atlas.Party.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Party.Infrastructure.Readers.Lookups.LookupGenders;

public sealed class LookupGendersReader(PartyDbContext db) : ILookupGendersReader
{
    private const string Sql = """
        SELECT 'Male' AS Code
        UNION ALL SELECT 'Female'
        UNION ALL SELECT 'PreferNotToSay'
        """;

    public async Task<IReadOnlyList<GenderLookupDto>> LookupAsync(CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var rows = await conn.QueryAsync<GenderLookupDto>(Sql);
        return rows.ToList();
    }
}
