using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Modules;

namespace Atlas.Outbox.Infrastructure.Readers.ListOutboxMessages;

public sealed class IdentityListOutboxMessagesReader(IdentityDbContext db)
    : ListOutboxMessagesReader(db, "atlas_identity", AtlasModules.Identity);
