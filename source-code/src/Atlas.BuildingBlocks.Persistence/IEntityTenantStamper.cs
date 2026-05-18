using Microsoft.EntityFrameworkCore;

namespace Atlas.BuildingBlocks.Persistence;

public interface IEntityTenantStamper
{
    void Stamp(DbContext db);
}
