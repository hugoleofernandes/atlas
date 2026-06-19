namespace Atlas.BuildingBlocks.Permissions;

/// <summary>
/// DB adapter de baixo nível para o catálogo de permissões.
///
/// ⚠️ NÃO injetar diretamente em código de application ou infrastructure.
/// Este reader é um detalhe de implementação usado exclusivamente por
/// <see cref="IPermissionCatalogCache"/> no cache miss.
///
/// Para qualquer uso em handlers, seeders ou endpoints: injetar <see cref="IPermissionCatalogCache"/>.
/// Não está registado como <see cref="IPermissionCatalogReader"/> no DI — tentativa de injetar
/// pela interface resultará em erro de DI por design.
/// </summary>
public interface IPermissionCatalogReader
{
    Task<PermissionRecord?> FindByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<PermissionRecord>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken ct);
    Task<IReadOnlyList<PermissionRecord>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);
    Task<IReadOnlyList<PermissionRecord>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<PermissionRecord>> GetAllActiveAsync(CancellationToken ct);
}
