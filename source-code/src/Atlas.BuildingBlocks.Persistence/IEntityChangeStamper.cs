using Microsoft.EntityFrameworkCore;

namespace Atlas.BuildingBlocks.Persistence;

public interface IEntityChangeStamper
{
    void Stamp(DbContext db);
}
