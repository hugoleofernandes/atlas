using Atlas.Platform.Application.Queries.EntityTypes.Lookup;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Platform.Infrastructure.Readers.EntityTypes.Lookup;

public sealed class LookupEntityTypesReader(PlatformDbContext db) : ILookupEntityTypesReader
{
    private const string Sql = """
        SELECT
            et.id   AS EntityTypeId,
            et.name AS EntityTypeName,
            m.id    AS ModuleId,
            m.name  AS ModuleName
        FROM atlas_platform.entity_types et
        JOIN atlas_platform.modules m ON m.id = et.module_id
        WHERE et.is_active = true
          AND m.is_active  = true
        ORDER BY m.name ASC, et.name ASC
        """;

    public async Task<IReadOnlyList<EntityTypeLookupDto>> LookupAsync(CancellationToken ct)
    {
        var conn    = db.Database.GetDbConnection();
        var results = await conn.QueryAsync<EntityTypeLookupDto>(Sql);
        return results.ToList().AsReadOnly();
    }
}
