using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.BuildingBlocks.Persistence.Entities.OutboxMessages.Configurations;

public sealed class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("outboxes");

        b.HasKey(x => x.Id);

        // ── Attempt-Chain ────────────────────────────────────────────────────
        // Each row is one processing attempt. On failure the current row is
        // closed (FailedAt set) and a child row is inserted with AttemptNumber+1.
        // ParentOutboxMessageId links the chain: C → B → A (null at the root).
        //
        // OnDelete(SetNull): if a parent row is deleted the child's FK is set to
        // null rather than cascading, preventing accidental deletion of history.
        //
        // TODO: add a background cleanup job that archives/deletes rows where
        //       processed_on < NOW() - INTERVAL '30 days' (or equivalent for
        //       failed/dead-lettered rows). Implement when volume warrants it.
        b.Property(x => x.ParentOutboxMessageId);

        b.HasOne<OutboxMessage>()
            .WithMany()
            .HasForeignKey(x => x.ParentOutboxMessageId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Property(x => x.AttemptNumber)
            .IsRequired()
            .HasDefaultValue(1);

        // ── Idempotency ──────────────────────────────────────────────────────
        // Shared across all retry rows for the same logical event.
        // Non-unique: all attempts in a chain carry the same key.
        b.Property(x => x.IdempotencyKey)
            .IsRequired();

        b.HasIndex(x => x.IdempotencyKey)
            .IsUnique(false);

        // ── Core event data ──────────────────────────────────────────────────
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

        // W3C traceparent — "00-{32-hex traceId}-{16-hex spanId}-{2-hex flags}" = 55 chars max.
        // Nullable: not set when there is no active OTel Activity (seeding, tests, CLI tools).
        b.Property(x => x.TraceParent)
            .HasMaxLength(55)
            .IsRequired(false);

        b.Property(x => x.OccurredOn)
            .IsRequired();

        b.Property(x => x.TenantId);
        b.Property(x => x.UserId);

        // Snapshot of the actor's email at publish time — nullable (seeding, CLI tools).
        // RFC 5321 max email length = 254 chars.
        b.Property(x => x.UserEmail)
            .HasMaxLength(254)
            .IsRequired(false);

        // ── State ────────────────────────────────────────────────────────────
        b.Property(x => x.ProcessedOn);
        b.Property(x => x.FailedAt);
        b.Property(x => x.DeadLetteredOn);

        // Short human-readable failure summary for quick debugging without a join.
        // Full per-handler detail lives in outbox_handler_executions.
        b.Property(x => x.Error);

        // ── Lock ─────────────────────────────────────────────────────────────
        b.Property(x => x.LockId);
        b.Property(x => x.LockedUntil);

        // ── Indexes ──────────────────────────────────────────────────────────

        // Primary polling index — covers the WHERE used by GetPendingBatchAsync.
        // Includes FailedAt so closed-as-failed rows are excluded efficiently.
        b.HasIndex(x => new
        {
            x.ProcessedOn,
            x.DeadLetteredOn,
            x.FailedAt,
            x.LockedUntil,
            x.OccurredOn
        });

        // Chain navigation — find children of a given parent quickly.
        b.HasIndex(x => x.ParentOutboxMessageId);

        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.Type);
        b.HasIndex(x => x.Module);
    }
}