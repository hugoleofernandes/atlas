using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Atlas.BuildingBlocks.Persistence.Audits;

public sealed class AuditService : IAuditService
{
    private readonly IRequestContext _ctx;

    public AuditService(IRequestContext ctx)
    {
        _ctx = ctx;
    }

    public async Task AddAuditLogsAsync(DbContext db, CancellationToken ct)
    {
        var tenantId = _ctx.TenantId;
        if (tenantId is null)
            return;

        db.ChangeTracker.DetectChanges();

        var entries = db.ChangeTracker.Entries()
            .Where(e =>
                e.Entity is not INotAuditable &&
                e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

        var logs = new List<Audit>();

        foreach (var entry in entries)
        {
            var changes = new Dictionary<string, object?>();

            foreach (var prop in entry.Properties)
            {
                if (prop.IsTemporary)
                    continue;

                if (entry.State == EntityState.Modified &&
                    Equals(prop.OriginalValue, prop.CurrentValue))
                    continue;

                changes[prop.Metadata.Name] = new
                {
                    Old = entry.State == EntityState.Added ? null : prop.OriginalValue,
                    New = entry.State == EntityState.Deleted ? null : prop.CurrentValue
                };
            }

            if (changes.Count == 0)
                continue;

            var audit = new Audit();
            audit.Initialize(
                entry.Entity.GetType().Name,
                entry.State.ToString(),
                GetPrimaryKey(entry),
                _ctx.UserId?.ToString(),
                tenantId.Value,
                JsonSerializer.Serialize(changes)
            );

            logs.Add(audit);
        }

        if (logs.Count > 0)
            await db.Set<Audit>().AddRangeAsync(logs, ct);
    }

    private static string? GetPrimaryKey(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key == null) return null;

        return string.Join(",",
            key.Properties.Select(p => entry.Property(p.Name).CurrentValue));
    }
}
