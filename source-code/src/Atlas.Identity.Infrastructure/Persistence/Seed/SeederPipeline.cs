using Atlas.Identity.Infrastructure.Persistence.DbContexts;

namespace Atlas.Identity.Infrastructure.Persistence.Seed;

public sealed class SeederPipeline
{
    private readonly IEnumerable<ISeeder> _seeders;

    public SeederPipeline(IEnumerable<ISeeder> seeders)
    {
        _seeders = seeders;
    }

    public async Task RunAsync(IdentityDbContext db, IServiceProvider services)
    {
        foreach (var seeder in _seeders)
        {
            await seeder.SeedAsync(db, services);
        }
    }
}
