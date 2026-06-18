using Atlas.Platform.Application.Queries.Lookups.LookupStatuses;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Platform.Infrastructure.Readers.Lookups.LookupStatuses;

public sealed class LookupStatusesReader(PlatformDbContext db) : ILookupStatusesReader
{
    private const string Sql = """
        SELECT 'Active' AS Code
        UNION ALL SELECT 'Inactive'
        UNION ALL SELECT 'All'
        """;

    public async Task<IReadOnlyList<StatusLookupDto>> LookupAsync(CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var rows = await conn.QueryAsync<StatusLookupDto>(Sql);
        return rows.ToList();
    }
}
