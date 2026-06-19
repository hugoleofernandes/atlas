using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Atlas.Party.Infrastructure.Persistence.DbContexts;

namespace Atlas.Party.Infrastructure.Readers.Audit.ListEntries;

public sealed class PartyAuditEntriesReader(PartyDbContext db)
    : BaseAuditEntriesReader(db, "atlas_party");
