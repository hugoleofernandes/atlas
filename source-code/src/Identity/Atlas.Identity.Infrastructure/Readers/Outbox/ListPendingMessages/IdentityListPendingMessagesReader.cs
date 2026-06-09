using Atlas.BuildingBlocks.Outbox.ListPendingMessages;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;

namespace Atlas.Identity.Infrastructure.Readers.Outbox.GetPendingMessages;

public sealed class IdentityListPendingMessagesReader(IdentityDbContext db)
    : ListPendingMessagesReader(db, "atlas_identity");
