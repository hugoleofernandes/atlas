using Atlas.SharedKernel.Application.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("outboxes");

        b.HasKey(x => x.Id);

        b.Property(x => x.Type)
            .HasMaxLength(300)
            .IsRequired();

        b.Property(x => x.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        b.Property(x => x.Module)
            .HasMaxLength(50)
            .IsRequired();

        b.Property(x => x.TenantId);

        b.Property(x => x.UserId);

        b.Property(x => x.CorrelationId)
            .HasMaxLength(100);

        b.Property(x => x.OccurredOn)
            .IsRequired();

        b.Property(x => x.ProcessedOn);

        b.HasIndex(x => new { x.ProcessedOn, x.OccurredOn });

        b.HasIndex(x => x.TenantId);
    }
}