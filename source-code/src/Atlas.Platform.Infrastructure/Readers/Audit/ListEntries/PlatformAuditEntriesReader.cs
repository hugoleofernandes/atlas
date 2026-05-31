using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;

namespace Atlas.Platform.Infrastructure.Readers.Audit.ListEntries;

public sealed class PlatformAuditEntriesReader(PlatformDbContext db)
    : BaseAuditEntriesReader(db, "atlas_platform");
