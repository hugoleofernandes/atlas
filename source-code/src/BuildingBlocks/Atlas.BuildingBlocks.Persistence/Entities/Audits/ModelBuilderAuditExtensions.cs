using Atlas.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace Atlas.BuildingBlocks.Persistence.Entities.Audits;

public static class ModelBuilderAuditExtensions
{
    public static void ValidateAuditableAggregates(this ModelBuilder modelBuilder)
    {
        var invalidTypes = modelBuilder.Model
            .GetEntityTypes()
            .Where(entityType =>
                entityType.ClrType is not null &&
                typeof(IAuditableAggregate).IsAssignableFrom(entityType.ClrType) &&
                entityType.FindAnnotation(AuditMetadataAnnotations.EntityTypeId)?.Value is not Guid)
            .Select(entityType => entityType.ClrType.Name)
            .OrderBy(name => name)
            .ToList();

        if (invalidTypes.Count == 0)
            return;

        throw new InvalidOperationException(
            $"Auditable aggregates missing EF annotation '{AuditMetadataAnnotations.EntityTypeId}': {string.Join(", ", invalidTypes)}.");
    }
}
