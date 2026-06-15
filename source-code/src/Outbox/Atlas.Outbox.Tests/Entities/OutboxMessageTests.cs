using Atlas.SharedKernel.Application.OutboxMessages;
using FluentAssertions;

namespace Atlas.Outbox.Tests.Entities;

/// <summary>
/// Pure unit tests for <see cref="OutboxMessage"/> — no mocks, no I/O, no async.
/// Every test exercises state transitions on the real entity.
/// </summary>
public sealed class OutboxMessageTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static OutboxMessage Create(string module = "tests") =>
        new("event.name", "Atlas.Tests.FakeEvent", """{"value":1}""",
            tenantId: Guid.NewGuid(), userId: Guid.NewGuid(),
            userEmail: null, correlationId: "corr-123", module: module);

    // ── 1. first attempt defaults ────────────────────────────────────────────

    [Fact]
    public void FirstAttempt_HasCorrectDefaults()
    {
        var msg = Create();

        msg.AttemptNumber.Should().Be(1);
        msg.IdempotencyKey.Should().Be(msg.Id);
        msg.ParentOutboxMessageId.Should().BeNull();
        msg.IsProcessed.Should().BeFalse();
        msg.IsDeadLettered.Should().BeFalse();
        msg.IsFailed.Should().BeFalse();
        msg.LockId.Should().BeNull();
        msg.LockedUntil.Should().BeNull();
        msg.Error.Should().BeNull();
    }

    // ── 2. MarkAsProcessed ───────────────────────────────────────────────────

    [Fact]
    public void MarkAsProcessed_SetsProcessedOn_ClearsLock()
    {
        var msg = Create();
        msg.TryLock(Guid.NewGuid(), TimeSpan.FromMinutes(1));

        var before = DateTime.UtcNow;
        msg.MarkAsProcessed();
        var after = DateTime.UtcNow;

        msg.IsProcessed.Should().BeTrue();
        msg.ProcessedOn.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        msg.LockId.Should().BeNull();
        msg.LockedUntil.Should().BeNull();
    }

    // ── 3. MarkAsDeadLettered ────────────────────────────────────────────────

    [Fact]
    public void MarkAsDeadLettered_SetsDeadLetteredOn_ClearsLock()
    {
        var msg = Create();
        msg.TryLock(Guid.NewGuid(), TimeSpan.FromMinutes(1));

        var before = DateTime.UtcNow;
        msg.MarkAsDeadLettered();
        var after = DateTime.UtcNow;

        msg.IsDeadLettered.Should().BeTrue();
        msg.DeadLetteredOn.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        msg.LockId.Should().BeNull();
        msg.LockedUntil.Should().BeNull();
    }

    // ── 4. CreateRetryAttempt — closes parent ────────────────────────────────

    [Fact]
    public void CreateRetryAttempt_ClosesParent()
    {
        var msg = Create();
        msg.TryLock(Guid.NewGuid(), TimeSpan.FromMinutes(1));

        var before = DateTime.UtcNow;
        msg.CreateRetryAttempt("some handlers failed");
        var after = DateTime.UtcNow;

        msg.IsFailed.Should().BeTrue();
        msg.FailedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        msg.Error.Should().Be("some handlers failed");
        msg.LockId.Should().BeNull();
        msg.LockedUntil.Should().BeNull();
    }

    // ── 5. CreateRetryAttempt — child chain fields ───────────────────────────

    [Fact]
    public void CreateRetryAttempt_ChildHasCorrectChainFields()
    {
        var parent = Create();

        var child = parent.CreateRetryAttempt();

        child.Id.Should().NotBe(parent.Id);
        child.AttemptNumber.Should().Be(2);
        child.IdempotencyKey.Should().Be(parent.IdempotencyKey);
        child.ParentOutboxMessageId.Should().Be(parent.Id);
        child.IsProcessed.Should().BeFalse();
        child.IsDeadLettered.Should().BeFalse();
        child.IsFailed.Should().BeFalse();
    }

    // ── 6. CreateRetryAttempt — child preserves event data ───────────────────

    [Fact]
    public void CreateRetryAttempt_ChildPreservesEventData()
    {
        var parent = Create(module: "identity");

        var child = parent.CreateRetryAttempt();

        child.Name.Should().Be(parent.Name);
        child.Type.Should().Be(parent.Type);
        child.Payload.Should().Be(parent.Payload);
        child.TenantId.Should().Be(parent.TenantId);
        child.UserId.Should().Be(parent.UserId);
        child.CorrelationId.Should().Be(parent.CorrelationId);
        child.Module.Should().Be(parent.Module);
    }

    // ── 7. Chain of 3 — attempt numbers and idempotency key intact ───────────

    [Fact]
    public void ChainOf3_AttemptNumbersIncrementAndIdempotencyKeyIsStable()
    {
        var attempt1 = Create();
        var originalKey = attempt1.IdempotencyKey;

        var attempt2 = attempt1.CreateRetryAttempt();
        var attempt3 = attempt2.CreateRetryAttempt();

        attempt1.AttemptNumber.Should().Be(1);
        attempt2.AttemptNumber.Should().Be(2);
        attempt3.AttemptNumber.Should().Be(3);

        attempt2.IdempotencyKey.Should().Be(originalKey);
        attempt3.IdempotencyKey.Should().Be(originalKey);

        attempt2.ParentOutboxMessageId.Should().Be(attempt1.Id);
        attempt3.ParentOutboxMessageId.Should().Be(attempt2.Id);
    }

    // ── 8–10. IsMaxAttemptReached ────────────────────────────────────────────

    [Fact]
    public void IsMaxAttemptReached_TrueWhenEqual()
    {
        var msg = Create(); // AttemptNumber = 1

        msg.IsMaxAttemptReached(maxRetries: 1).Should().BeTrue();
    }

    [Fact]
    public void IsMaxAttemptReached_TrueWhenExceeds()
    {
        var attempt1 = Create();
        var attempt2 = attempt1.CreateRetryAttempt(); // AttemptNumber = 2
        var attempt3 = attempt2.CreateRetryAttempt(); // AttemptNumber = 3

        attempt3.IsMaxAttemptReached(maxRetries: 2).Should().BeTrue();
    }

    [Fact]
    public void IsMaxAttemptReached_FalseWhenBelow()
    {
        var msg = Create(); // AttemptNumber = 1

        msg.IsMaxAttemptReached(maxRetries: 3).Should().BeFalse();
    }

    // ── 11. TryLock ──────────────────────────────────────────────────────────

    [Fact]
    public void TryLock_ReturnsFalse_WhenAlreadyLocked()
    {
        var msg = Create();
        var first = msg.TryLock(Guid.NewGuid(), TimeSpan.FromMinutes(1));

        var second = msg.TryLock(Guid.NewGuid(), TimeSpan.FromMinutes(1));

        first.Should().BeTrue();
        second.Should().BeFalse();
    }

    // ── 12. IsLocked with expired lock ───────────────────────────────────────

    [Fact]
    public void IsLocked_ReturnsFalse_WhenLockDurationIsNegative()
    {
        var msg = Create();
        // A negative duration sets LockedUntil to a point in the past —
        // simulates a lock that expired before we even check.
        msg.TryLock(Guid.NewGuid(), TimeSpan.FromMilliseconds(-1));

        msg.IsLocked().Should().BeFalse();
    }

    // ── 13. CreateResubmissionAttempt — new row fields ───────────────────────

    private static OutboxMessage CreateDeadLettered()
    {
        var msg = Create();
        msg.MarkAsDeadLettered();
        return msg;
    }

    [Fact]
    public void CreateResubmissionAttempt_ResetsAttemptNumberTo1()
    {
        var dead = CreateDeadLettered();
        var userId = Guid.NewGuid();

        var replay = dead.CreateResubmissionAttempt(userId, "op@example.com");

        replay.AttemptNumber.Should().Be(1);
    }

    [Fact]
    public void CreateResubmissionAttempt_PreservesIdempotencyKeyAndLinksParent()
    {
        var dead = CreateDeadLettered();
        var userId = Guid.NewGuid();

        var replay = dead.CreateResubmissionAttempt(userId, "op@example.com");

        replay.IdempotencyKey.Should().Be(dead.IdempotencyKey);
        replay.ParentOutboxMessageId.Should().Be(dead.Id);
        replay.Id.Should().NotBe(dead.Id);
    }

    [Fact]
    public void CreateResubmissionAttempt_SetsOriginAndAuthorship()
    {
        var dead = CreateDeadLettered();
        var userId = Guid.NewGuid();
        const string email = "op@example.com";

        var replay = dead.CreateResubmissionAttempt(userId, email);

        replay.Origin.Should().Be(OutboxMessageOrigin.ManualResubmit);
        replay.ResubmittedByUserId.Should().Be(userId);
        replay.ResubmittedByEmail.Should().Be(email);
    }

    [Fact]
    public void CreateResubmissionAttempt_DoesNotMutateDeadLetteredRow()
    {
        var dead = CreateDeadLettered();
        var deadLetteredOn = dead.DeadLetteredOn;

        dead.CreateResubmissionAttempt(Guid.NewGuid(), "op@example.com");

        dead.IsDeadLettered.Should().BeTrue();
        dead.DeadLetteredOn.Should().Be(deadLetteredOn);
        dead.IsFailed.Should().BeFalse();
        dead.IsProcessed.Should().BeFalse();
    }

    [Fact]
    public void CreateResubmissionAttempt_Throws_WhenNotDeadLettered()
    {
        var msg = Create();

        var act = () => msg.CreateResubmissionAttempt(Guid.NewGuid(), "op@example.com");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CreateResubmissionAttempt_Throws_WhenAlreadyProcessed()
    {
        var msg = Create();
        msg.MarkAsProcessed();

        var act = () => msg.CreateResubmissionAttempt(Guid.NewGuid(), "op@example.com");

        act.Should().Throw<InvalidOperationException>();
    }

    // ── 14. CreateRetryAttempt propagates Origin and authorship ──────────────

    [Fact]
    public void CreateRetryAttempt_PropagatesOriginAndAuthorship()
    {
        var dead = CreateDeadLettered();
        var userId = Guid.NewGuid();
        const string email = "op@example.com";

        var replay = dead.CreateResubmissionAttempt(userId, email);
        var retry  = replay.CreateRetryAttempt("handler failed");

        retry.Origin.Should().Be(OutboxMessageOrigin.ManualResubmit);
        retry.ResubmittedByUserId.Should().Be(userId);
        retry.ResubmittedByEmail.Should().Be(email);
    }

    // ── 15. Automatic attempt defaults ───────────────────────────────────────

    [Fact]
    public void FirstAttempt_HasAutomaticOrigin()
    {
        var msg = Create();

        msg.Origin.Should().Be(OutboxMessageOrigin.Automatic);
        msg.ResubmittedByUserId.Should().BeNull();
        msg.ResubmittedByEmail.Should().BeNull();
    }

    [Fact]
    public void ResolveOutcomeOn_PrefersProcessedOn()
    {
        var processedOn = DateTime.UtcNow;
        var failedAt = processedOn.AddMinutes(1);
        var deadLetteredOn = processedOn.AddMinutes(2);

        var outcomeOn = OutboxMessage.ResolveOutcomeOn(processedOn, failedAt, deadLetteredOn);

        outcomeOn.Should().Be(processedOn);
    }

    [Fact]
    public void ResolveOutcomeOn_UsesFailedAt_WhenProcessedOnIsNull()
    {
        var failedAt = DateTime.UtcNow;
        var deadLetteredOn = failedAt.AddMinutes(1);

        var outcomeOn = OutboxMessage.ResolveOutcomeOn(null, failedAt, deadLetteredOn);

        outcomeOn.Should().Be(failedAt);
    }

    [Fact]
    public void ResolveOutcomeOn_UsesDeadLetteredOn_WhenOtherOutcomesAreNull()
    {
        var deadLetteredOn = DateTime.UtcNow;

        var outcomeOn = OutboxMessage.ResolveOutcomeOn(null, null, deadLetteredOn);

        outcomeOn.Should().Be(deadLetteredOn);
    }
}
