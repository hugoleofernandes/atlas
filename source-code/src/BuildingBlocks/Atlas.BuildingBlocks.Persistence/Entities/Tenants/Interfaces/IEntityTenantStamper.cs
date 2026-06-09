using Microsoft.EntityFrameworkCore;

namespace Atlas.BuildingBlocks.Persistence.Entities.Tenants.Interfaces;

public interface IEntityTenantStamper
{
    void Stamp(DbContext db);
}
