namespace Atlas.Identity.Infrastructure.Persistence.Seed;

public interface ISeeder
{
    Task SeedAsync(AtlasDbContext db, IServiceProvider services);
}