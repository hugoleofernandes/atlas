using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace Atlas.BuildingBlocks.Persistence.Audits;

public sealed class EntityChangeStamper : IEntityChangeStamper
{
    private readonly IRequestContext _requestContext;

    public EntityChangeStamper(IRequestContext requestContext)
    {
        _requestContext = requestContext;
    }

    public void Stamp(DbContext db)
    {
        var now = DateTime.UtcNow;
        var userId = _requestContext.UserId;
        var userEmail = _requestContext.UserEmail;

        foreach (var entry in db.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.SetCreated(now, userId, userEmail);

            else if (entry.State == EntityState.Modified)
                entry.Entity.SetUpdated(now, userId, userEmail);
        }
    }
}
