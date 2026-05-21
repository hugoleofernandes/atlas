using Microsoft.EntityFrameworkCore;

namespace Atlas.BuildingBlocks.Persistence.Entities.EntityChanges.Interfaces;

public interface IEntityChangeStamper
{
    void Stamp(DbContext db);
}
