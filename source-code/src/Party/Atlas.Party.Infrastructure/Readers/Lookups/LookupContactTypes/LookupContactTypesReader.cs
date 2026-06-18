using Atlas.Party.Application.Queries.Lookups.LookupContactTypes;
using Atlas.Party.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Party.Infrastructure.Readers.Lookups.LookupContactTypes;

public sealed class LookupContactTypesReader(PartyDbContext db) : ILookupContactTypesReader
{
    private const string Sql = """
        SELECT 'Email'    AS Code, 'Email'    AS Name
        UNION ALL SELECT 'Phone',    'Phone'
        UNION ALL SELECT 'Mobile',   'Mobile'
        UNION ALL SELECT 'WhatsApp', 'WhatsApp'
        UNION ALL SELECT 'Other',    'Other'
        """;

    public async Task<IReadOnlyList<ContactTypeLookupDto>> LookupAsync(CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var rows = await conn.QueryAsync<ContactTypeLookupDto>(Sql);
        return rows.ToList();
    }
}
