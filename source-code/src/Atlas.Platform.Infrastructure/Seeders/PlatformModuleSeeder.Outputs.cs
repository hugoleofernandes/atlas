namespace Atlas.Platform.Infrastructure.Seeders;

public sealed partial class PlatformModuleSeeder
{
    private sealed record ModuleSeedOutput(IReadOnlyDictionary<Guid, Guid> ModuleIdsByCatalogId);
}
