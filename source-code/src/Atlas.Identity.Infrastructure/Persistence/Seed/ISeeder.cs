namespace Atlas.Identity.Infrastructure.Persistence.Seed;

public interface ISeeder
{
    Task SeedAsync(IdentityDbContext db, IServiceProvider services);
}