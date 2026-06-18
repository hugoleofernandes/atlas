using Atlas.Party.Application.Queries.Lookups.LookupAddressTypes;
using Atlas.Party.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Party.Infrastructure.Readers.Lookups.LookupAddressTypes;

public sealed class LookupAddressTypesReader(PartyDbContext db) : ILookupAddressTypesReader
{
    private const string Sql = """
        SELECT 'Main'     AS Code, 'Main'     AS Name
        UNION ALL SELECT 'Billing',  'Billing'
        UNION ALL SELECT 'Delivery', 'Delivery'
        UNION ALL SELECT 'Other',    'Other'
        """;

    public async Task<IReadOnlyList<AddressTypeLookupDto>> LookupAsync(CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var rows = await conn.QueryAsync<AddressTypeLookupDto>(Sql);
        return rows.ToList();
    }
}
