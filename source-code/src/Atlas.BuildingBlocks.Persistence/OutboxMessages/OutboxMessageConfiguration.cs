using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.BuildingBlocks.Persistence.OutboxMessages;

public sealed class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("outboxes");

        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Type)
            .HasMaxLength(300)
            .IsRequired();

        b.Property(x => x.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        b.Property(x => x.Module)
            .HasMaxLength(50)
            .IsRequired();

        b.Property(x => x.CorrelationId)
            .HasMaxLength(100);

        b.Property(x => x.OccurredOn)
            .IsRequired();

        b.Property(x => x.ProcessedOn);

        b.Property(x => x.RetryCount)
            .IsRequired();

        b.Property(x => x.Error);

        b.Property(x => x.LockId);

        b.Property(x => x.LockedUntil);

        b.Property(x => x.DeadLetteredOn);

        b.Property(x => x.TenantId);
        b.Property(x => x.UserId);

        // -------------------------
        // INDEXES
        // -------------------------

        b.HasIndex(x => new
        {
            x.ProcessedOn,
            x.DeadLetteredOn,
            x.LockedUntil,
            x.OccurredOn
        });

        b.HasIndex(x => x.TenantId);

        b.HasIndex(x => x.Type);

        b.HasIndex(x => x.Module);
    }
}