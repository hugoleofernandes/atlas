using Atlas.Party.Application.Queries.Lookups.LookupClassificationTypes;
using Atlas.Party.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Party.Infrastructure.Readers.Lookups.LookupClassificationTypes;

public sealed class LookupClassificationTypesReader(PartyDbContext db) : ILookupClassificationTypesReader
{
    private const string Sql = """
        SELECT 'Staff' AS Code
        UNION ALL
        SELECT 'Customer'
        """;

    public async Task<IReadOnlyList<ClassificationTypeLookupDto>> LookupAsync(CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var rows = await conn.QueryAsync<ClassificationTypeLookupDto>(Sql);
        return rows.ToList();
    }
}
