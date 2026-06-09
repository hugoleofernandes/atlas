using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.BuildingBlocks.Persistence.Entities.OutboxMessages.Configurations;

public sealed class OutboxHandlerExecutionConfiguration
    : IEntityTypeConfiguration<OutboxHandlerExecution>
{
    public void Configure(EntityTypeBuilder<OutboxHandlerExecution> b)
    {
        b.ToTable("outbox_handler_executions");

        b.HasKey(x => x.Id);

        // ── Relationship ──────────────────────────────────────────────────────
        // Cascade: when the parent OutboxMessage is deleted by the cleanup job,
        // its execution records are removed automatically — no orphans.
        b.Property(x => x.OutboxMessageId)
            .IsRequired();

        b.HasOne<OutboxMessage>()
            .WithMany()
            .HasForeignKey(x => x.OutboxMessageId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Columns ───────────────────────────────────────────────────────────
        b.Property(x => x.HandlerName)
            .HasMaxLength(300)
            .IsRequired();

        b.Property(x => x.Status)
            .HasMaxLength(20)
            .IsRequired();

        b.Property(x => x.ErrorMessage);

        b.Property(x => x.AttemptedAt)
            .IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────

        // Primary lookup: all executions for a given attempt row.
        b.HasIndex(x => x.OutboxMessageId);

        // Operational queries: how many times did handler X fail?
        b.HasIndex(x => new { x.HandlerName, x.Status });

        // Time-range queries for dashboards and alerting.
        b.HasIndex(x => x.AttemptedAt);
    }
}
