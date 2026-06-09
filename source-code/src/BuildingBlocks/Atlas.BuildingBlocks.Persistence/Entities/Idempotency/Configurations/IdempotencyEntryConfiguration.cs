using Atlas.BuildingBlocks.Persistence.Entities.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.BuildingBlocks.Persistence.Entities.Idempotency.Configurations;

public sealed class IdempotencyEntryConfiguration : IEntityTypeConfiguration<IdempotencyEntry>
{
    public void Configure(EntityTypeBuilder<IdempotencyEntry> b)
    {
        b.ToTable("idempotency_entries");

        // Composite PK doubles as the unique constraint used by INSERT ON CONFLICT.
        b.HasKey(x => new { x.IdempotencyKey, x.HandlerName });

        b.Property(x => x.IdempotencyKey)
            .IsRequired();

        b.Property(x => x.HandlerName)
            .IsRequired()
            .HasMaxLength(500);

        b.Property(x => x.ProcessedAt)
            .IsRequired();
    }
}
