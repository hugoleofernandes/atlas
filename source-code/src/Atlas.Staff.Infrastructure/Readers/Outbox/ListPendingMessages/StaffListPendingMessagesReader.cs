using Atlas.BuildingBlocks.Outbox.ListPendingMessages;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;

namespace Atlas.Staff.Infrastructure.Readers.Outbox.ListPendingMessages;

public sealed class StaffListPendingMessagesReader(StaffDbContext db) : ListPendingMessagesReader(db, "atlas_staff");
