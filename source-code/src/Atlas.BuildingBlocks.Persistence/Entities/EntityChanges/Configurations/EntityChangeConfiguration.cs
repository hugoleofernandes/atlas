using Atlas.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.BuildingBlocks.Persistence.Entities.EntityChanges.Configurations;

/// <summary>
/// Reusable EF Core configuration helper for entities implementing IAuditableEntity.
/// Call Configure(b) inside any IEntityTypeConfiguration to add audit columns consistently.
/// </summary>
public static class EntityChangeConfiguration
{
    public static void Configure<T>(EntityTypeBuilder<T> b)
        where T : class, IAuditableEntity
    {
        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.Property(x => x.CreatedBy);

        b.Property(x => x.CreatedByEmail)
            .HasMaxLength(256);

        b.Property(x => x.UpdatedAt);

        b.Property(x => x.UpdatedBy);

        b.Property(x => x.UpdatedByEmail)
            .HasMaxLength(256);
    }
}
