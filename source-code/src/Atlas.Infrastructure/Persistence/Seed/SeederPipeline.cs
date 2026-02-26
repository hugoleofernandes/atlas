using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Infrastructure.Persistence.Seed;

public sealed class SeederPipeline
{
    private readonly IEnumerable<ISeeder> _seeders;

    public SeederPipeline(IEnumerable<ISeeder> seeders)
    {
        _seeders = seeders;
    }

    public async Task RunAsync(AtlasDbContext db, IServiceProvider services)
    {
        foreach (var seeder in _seeders)
        {
            await seeder.SeedAsync(db, services);
        }
    }
}
