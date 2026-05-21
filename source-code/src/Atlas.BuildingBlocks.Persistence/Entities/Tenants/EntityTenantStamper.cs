using Atlas.BuildingBlocks.Persistence.Entities.Tenants.Interfaces;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace Atlas.BuildingBlocks.Persistence.Entities.Tenants;

public sealed class EntityTenantStamper : IEntityTenantStamper
{
    private readonly IRequestContext _requestContext;

    public EntityTenantStamper(IRequestContext requestContext)
    {
        _requestContext = requestContext;
    }

    public void Stamp(DbContext db)
    {
        if (_requestContext.TenantId is not { } tenantId)
            return;

        foreach (var entry in db.ChangeTracker.Entries<IMultiTenantEntity>())
        {
            if (entry.Entity is INotMultiTenant)
                continue;

            if (entry.State == EntityState.Added)
                entry.Entity.SetTenantId(tenantId);
        }
    }
}
