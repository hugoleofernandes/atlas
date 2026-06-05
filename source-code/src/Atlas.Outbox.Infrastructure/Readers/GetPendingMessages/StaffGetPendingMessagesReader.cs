using Atlas.Staff.Infrastructure.Persistence.DbContexts;

namespace Atlas.Outbox.Infrastructure.Readers.GetPendingMessages;

public sealed class StaffGetPendingMessagesReader(StaffDbContext db)
    : GetPendingMessagesReader(db, "atlas_staff");
