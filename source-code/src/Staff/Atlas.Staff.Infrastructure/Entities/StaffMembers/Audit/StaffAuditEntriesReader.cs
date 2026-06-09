using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;

namespace Atlas.Staff.Infrastructure.Entities.StaffMembers.Audit;

public sealed class StaffAuditEntriesReader(StaffDbContext db)
    : BaseAuditEntriesReader(db, "atlas_staff");
