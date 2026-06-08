using Atlas.Platform.Domain.Systems;
using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlas.Platform.Infrastructure.Seeders;

public sealed partial class PlatformModuleSeeder
{
    private async Task SeedSystemAsync(CancellationToken ct)
    {
        if (await db.Systems.AnyAsync(ct))
        {
            logger.LogInformation("PlatformSystemSeeder skipped - data already exists");
            return;
        }

        logger.LogInformation("PlatformSystemSeeder started");

        db.Systems.Add(AtlasSystem.Create("mlab"));

        setter.Set(Guid.NewGuid(), "platform-seed", SystemIdentity.UserId, SystemIdentity.Email);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("PlatformSystemSeeder completed");
    }
}
