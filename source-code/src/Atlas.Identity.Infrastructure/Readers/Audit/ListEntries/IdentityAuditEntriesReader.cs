using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;

namespace Atlas.Identity.Infrastructure.Readers.Audit.ListEntries;

public sealed class IdentityAuditEntriesReader(IdentityDbContext db)
    : BaseAuditEntriesReader(db, "atlas_identity");
