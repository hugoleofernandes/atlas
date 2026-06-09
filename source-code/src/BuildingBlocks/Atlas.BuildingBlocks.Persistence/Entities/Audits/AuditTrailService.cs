using Atlas.BuildingBlocks.Persistence.Entities.Audits.Interfaces;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Atlas.BuildingBlocks.Persistence.Entities.Audits;

public sealed class AuditTrailService : IAuditTrailService
{
    private static readonly HashSet<string> _excludedProperties =
        typeof(IAuditableEntity)
            .GetProperties()
            .Concat(typeof(IMultiTenantEntity).GetProperties())
            .Select(p => p.Name)
            .ToHashSet();

    private readonly IRequestContext _ctx;

    public AuditTrailService(IRequestContext ctx)
    {
        _ctx = ctx;
    }

    public async Task RecordAsync(DbContext db, CancellationToken ct)
    {
        var tenantId = _ctx.TenantId;
        if (tenantId is null)
            return;

        db.ChangeTracker.DetectChanges();

        var entries = db.ChangeTracker.Entries()
            .Where(e =>
                e.Entity is IAuditableAggregate &&
                e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

        var logs = new List<Audit>();

        foreach (var entry in entries)
        {
            // Only entities that explicitly opt in to auditing are tracked.
            var auditableAggregate = (IAuditableAggregate)entry.Entity;

            var changes = new Dictionary<string, object?>();

            foreach (var prop in entry.Properties)
            {
                if (prop.IsTemporary)
                    continue;

                if (_excludedProperties.Contains(prop.Metadata.Name))
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
                auditableAggregate.EntityTypeId,
                entry.State.ToString(),
                GetPrimaryKey(entry),
                _ctx.UserId?.ToString(),
                _ctx.UserEmail,
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
