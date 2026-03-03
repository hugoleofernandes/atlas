using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Atlas.BuildingBlocks.Persistence;

public abstract class AuditableDbContext<TAudit>
    : MultiTenantDbContext
    where TAudit : class, IAuditLog, new()
{
    private readonly IRequestContext _requestContext;

    protected AuditableDbContext(
        DbContextOptions options,
        IRequestContext requestContext)
        : base(options, requestContext)
    {
        _requestContext = requestContext;
    }

    protected Guid? CurrentUserId => _requestContext.UserId;

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await AddAuditLogsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task AddAuditLogsAsync(CancellationToken ct)
    {
        if (CurrentTenantId is null)
            return; // ou throw se tenant for obrigatório

        ChangeTracker.DetectChanges();

        var entries = ChangeTracker.Entries()
            .Where(e =>
                e.Entity is not TAudit &&
                e.State is EntityState.Added
                    or EntityState.Modified
                    or EntityState.Deleted);

        var auditLogs = new List<TAudit>();

        foreach (var entry in entries)
        {
            var changes = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                    continue;

                if (entry.State == EntityState.Modified &&
                    Equals(property.OriginalValue, property.CurrentValue))
                    continue;

                changes[property.Metadata.Name] = new
                {
                    Old = entry.State == EntityState.Added ? null : property.OriginalValue,
                    New = entry.State == EntityState.Deleted ? null : property.CurrentValue
                };
            }

            if (changes.Count == 0)
                continue;

            var audit = new TAudit();

            audit.Initialize(
                entry.Entity.GetType().Name,
                entry.State.ToString(),
                GetPrimaryKey(entry),
                CurrentUserId?.ToString(),
                CurrentTenantId.Value,
                JsonSerializer.Serialize(changes)
            );

            auditLogs.Add(audit);
        }

        if (auditLogs.Count > 0)
            await Set<TAudit>().AddRangeAsync(auditLogs, ct);
    }

    private static string? GetPrimaryKey(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key == null) return null;

        return string.Join(",",
            key.Properties.Select(p => entry.Property(p.Name).CurrentValue));
    }
}