using Atlas.Identity.Infrastructure.Persistence.DbContexts;

namespace Atlas.Outbox.Infrastructure.Readers.GetPendingMessages;

public sealed class IdentityGetPendingMessagesReader(IdentityDbContext db)
    : GetPendingMessagesReader(db, "atlas_identity");
