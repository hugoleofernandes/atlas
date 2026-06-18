using Atlas.Party.Application.Queries.Lookups.LookupContactTypes;
using Atlas.Party.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Party.Infrastructure.Readers.Lookups.LookupContactTypes;

public sealed class LookupContactTypesReader(PartyDbContext db) : ILookupContactTypesReader
{
    private const string Sql = """
        SELECT 'Email' AS Code
        UNION ALL SELECT 'Phone'
        UNION ALL SELECT 'Mobile'
        UNION ALL SELECT 'WhatsApp'
        UNION ALL SELECT 'Other'
        """;

    public async Task<IReadOnlyList<ContactTypeLookupDto>> LookupAsync(CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var rows = await conn.QueryAsync<ContactTypeLookupDto>(Sql);
        return rows.ToList();
    }
}
