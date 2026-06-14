using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Modules;

namespace Atlas.Outbox.Infrastructure.Readers.ListOutboxMessages;

public sealed class StaffListOutboxMessagesReader(StaffDbContext db)
    : ListOutboxMessagesReader(db, "atlas_staff", AtlasModules.Staff);
