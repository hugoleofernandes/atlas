using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Readers.Permissions;

public sealed class PermissionCatalogReader(IdentityDbContext db) : IPermissionCatalogReader
{
    private const string FindByCodeSql = """
        SELECT id,
               module_id        AS ModuleId,
               module_name      AS ModuleName,
               code,
               "group"          AS Group,
               is_manager       AS IsManager,
               is_root          AS IsRoot,
               is_active        AS IsActive
        FROM atlas_identity.permissions
        WHERE code = @Code
        """;

    private const string GetByCodesSql = """
        SELECT id,
               module_id        AS ModuleId,
               module_name      AS ModuleName,
               code,
               "group"          AS Group,
               is_manager       AS IsManager,
               is_root          AS IsRoot,
               is_active        AS IsActive
        FROM atlas_identity.permissions
        WHERE code = ANY(@Codes)
        """;

    private const string GetByIdsSql = """
        SELECT id,
               module_id        AS ModuleId,
               module_name      AS ModuleName,
               code,
               "group"          AS Group,
               is_manager       AS IsManager,
               is_root          AS IsRoot,
               is_active        AS IsActive
        FROM atlas_identity.permissions
        WHERE id = ANY(@Ids)
        """;

    private const string GetAllActiveSql = """
        SELECT id,
               module_id        AS ModuleId,
               module_name      AS ModuleName,
               code,
               "group"          AS Group,
               is_manager       AS IsManager,
               is_root          AS IsRoot,
               is_active        AS IsActive
        FROM atlas_identity.permissions
        WHERE is_active = true
        """;

    public async Task<PermissionRecord?> FindByCodeAsync(string code, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        return await conn.QueryFirstOrDefaultAsync<PermissionRecord>(FindByCodeSql, new { Code = code });
    }

    public async Task<IReadOnlyList<PermissionRecord>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var results = await conn.QueryAsync<PermissionRecord>(GetByCodesSql, new { Codes = codes.ToArray() });
        return results.ToList();
    }

    public async Task<IReadOnlyList<PermissionRecord>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var results = await conn.QueryAsync<PermissionRecord>(GetByIdsSql, new { Ids = ids.ToArray() });
        return results.ToList();
    }

    public async Task<IReadOnlyList<PermissionRecord>> GetAllActiveAsync(CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var results = await conn.QueryAsync<PermissionRecord>(GetAllActiveSql);
        return results.ToList();
    }
}
