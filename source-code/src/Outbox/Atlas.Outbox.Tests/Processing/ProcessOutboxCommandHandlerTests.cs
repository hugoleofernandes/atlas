using Atlas.Outbox.Application.Commands.ProcessOutbox;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Dispatching;
using Atlas.SharedKernel.Application.OutboxMessages;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Atlas.Outbox.Tests.Processing;

/// <summary>
/// Unit tests for <see cref="ProcessOutboxCommandHandler"/>.
///
/// Strategy:
///   - <see cref="OutboxMessage"/> is used as the real entity — mocking it would hide
///     state-transition bugs, which is exactly what these tests are designed to catch.
///   - Repository, dispatcher, UoW and context setter are mocked via NSubstitute.
///   - Every test follows the AAA pattern (Arrange / Act / Assert).
/// </summary>
public sealed class ProcessOutboxCommandHandlerTests
{
    // ── infrastructure ───────────────────────────────────────────────────────

    private readonly IOutboxWorkerRepository _repository = Substitute.For<IOutboxWorkerRepository>();
    private readonly IOutboxMessageDispatcher _dispatcher = Substitute.For<IOutboxMessageDispatcher>();
    private readonly IDispatcherInvoker _dispatcherInvoker = Substitute.For<IDispatcherInvoker>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IRequestContextSetter _contextSetter = Substitute.For<IRequestContextSetter>();
    private readonly ITraceContextSetter _traceContextSetter = Substitute.For<ITraceContextSetter>();

    private readonly ProcessOutboxCommandHandler _sut;

    // MaxRetries = 3 → AttemptNumber 1 and 2 can retry; AttemptNumber 3 dead-letters.
    private static readonly ProcessOutboxCommand DefaultCommand = new(
        BatchSize: 10,
        MaxRetries: 3,
        LockDuration: TimeSpan.FromSeconds(30),
        Module: "test"
    );

    public ProcessOutboxCommandHandlerTests()
    {
        _repository.AddRetryAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _repository
            .AddExecutionsAsync(Arg.Any<IReadOnlyList<OutboxHandlerExecution>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _sut = new ProcessOutboxCommandHandler(
            _repository,
            _dispatcher,
            _dispatcherInvoker,
            _uow,
            _contextSetter,
            _traceContextSetter
        );
    }

    // ── factory helpers ──────────────────────────────────────────────────────

    private static OutboxMessage CreateMessage() =>
        new(
            "event.name",
            "Atlas.Tests.FakeEvent",
            """{"v":1}""",
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "corr-123",
            "tests"
        );

    /// <summary>
    /// Chains <paramref name="maxRetries"/> - 1 retry attempts so the returned message
    /// sits at <c>AttemptNumber == maxRetries</c> and <c>IsMaxAttemptReached</c> returns true.
    /// </summary>
    private static OutboxMessage CreateFinalAttemptMessage(int maxRetries = 3)
    {
        var msg = CreateMessage();
        for (var i = 1; i < maxRetries; i++)
            msg = msg.CreateRetryAttempt();
        return msg;
    }

    private void GivenBatch(params OutboxMessage[] messages) =>
        _repository
            .GetPendingBatchAsync(Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(messages.ToList());

    private void GivenDispatcherReturns(params HandlerInvocationResult[] results) =>
        _dispatcherInvoker
            .InvokeAsync(Arg.Any<IOutboxMessageDispatcher>(), Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>())
            .Returns(results.ToList());

    // ── 1. empty batch ───────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenBatchIsEmpty_ShouldReturnZeroOutputAndNeverDispatch()
    {
        // Arrange
        GivenBatch( /* empty */
        );

        // Act
        var output = await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert
        output.Should().Be(new ProcessOutboxOutput(0, 0, 0));
        await _dispatcherInvoker
            .DidNotReceive()
            .InvokeAsync(Arg.Any<IOutboxMessageDispatcher>(), Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>());
    }

    // ── 2–4. all handlers succeed ────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenAllHandlersSucceed_ShouldMarkMessageAsProcessed()
    {
        // Arrange
        var message = CreateMessage();
        GivenBatch(message);
        GivenDispatcherReturns(HandlerInvocationResult.Success("HandlerA"));

        // Act
        await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert
        message.IsProcessed.Should().BeTrue();
        message.IsFailed.Should().BeFalse();
        message.IsDeadLettered.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WhenAllHandlersSucceed_ShouldAddOneExecutionRecordPerHandler()
    {
        // Arrange
        var message = CreateMessage();
        GivenBatch(message);
        GivenDispatcherReturns(
            HandlerInvocationResult.Success("HandlerA"),
            HandlerInvocationResult.Success("HandlerB")
        );

        IReadOnlyList<OutboxHandlerExecution>? captured = null;
        _repository
            .AddExecutionsAsync(
                Arg.Do<IReadOnlyList<OutboxHandlerExecution>>(x => captured = x),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert
        captured.Should().HaveCount(2);
        captured.Should().AllSatisfy(e => e.Status.Should().Be("Success"));
        captured.Should().ContainSingle(e => e.HandlerName == "HandlerA");
        captured.Should().ContainSingle(e => e.HandlerName == "HandlerB");
    }

    [Fact]
    public async Task ExecuteAsync_WhenAllHandlersSucceed_ShouldNotAddRetry()
    {
        // Arrange
        var message = CreateMessage();
        GivenBatch(message);
        GivenDispatcherReturns(HandlerInvocationResult.Success("HandlerA"));

        // Act
        await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert
        await _repository.DidNotReceive().AddRetryAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>());
    }

    // ── 5–8. handler fails — non-final attempt ───────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenHandlerFailsOnNonFinalAttempt_ShouldCloseParentAndCreateRetry()
    {
        // Arrange
        var message = CreateMessage(); // AttemptNumber = 1, maxRetries = 3 → can retry
        GivenBatch(message);
        GivenDispatcherReturns(HandlerInvocationResult.Failure("HandlerA", "timeout"));

        // Act
        await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert
        message.IsFailed.Should().BeTrue("parent must be closed as failed");
        message.IsProcessed.Should().BeFalse();
        await _repository.Received(1).AddRetryAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerFailsOnNonFinalAttempt_ShouldRetryHaveIncrementedAttemptNumber()
    {
        // Arrange
        var message = CreateMessage(); // AttemptNumber = 1
        GivenBatch(message);
        GivenDispatcherReturns(HandlerInvocationResult.Failure("HandlerA", "timeout"));

        OutboxMessage? capturedRetry = null;
        _repository
            .AddRetryAsync(Arg.Do<OutboxMessage>(m => capturedRetry = m), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert
        capturedRetry.Should().NotBeNull();
        capturedRetry!.AttemptNumber.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerFailsOnNonFinalAttempt_ShouldRetryShareIdempotencyKey()
    {
        // Arrange
        var message = CreateMessage();
        GivenBatch(message);
        GivenDispatcherReturns(HandlerInvocationResult.Failure("HandlerA", "timeout"));

        OutboxMessage? capturedRetry = null;
        _repository
            .AddRetryAsync(Arg.Do<OutboxMessage>(m => capturedRetry = m), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert
        capturedRetry!
            .IdempotencyKey.Should()
            .Be(message.IdempotencyKey, "idempotency key must be stable across the entire attempt chain");
        capturedRetry.ParentOutboxMessageId.Should().Be(message.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerFailsOnNonFinalAttempt_ShouldAddFailureExecutionRecord()
    {
        // Arrange
        var message = CreateMessage();
        GivenBatch(message);
        GivenDispatcherReturns(HandlerInvocationResult.Failure("HandlerA", "db connection lost"));

        IReadOnlyList<OutboxHandlerExecution>? captured = null;
        _repository
            .AddExecutionsAsync(
                Arg.Do<IReadOnlyList<OutboxHandlerExecution>>(x => captured = x),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert
        captured.Should().ContainSingle();
        captured![0].HandlerName.Should().Be("HandlerA");
        captured![0].Status.Should().Be("Failure");
        captured![0].ErrorMessage.Should().Be("db connection lost");
    }

    [Fact]
    public async Task ExecuteAsync_WhenPartialHandlerFailure_ShouldAddBothSuccessAndFailureExecutionRecords()
    {
        // Arrange
        var message = CreateMessage();
        GivenBatch(message);
        GivenDispatcherReturns(
            HandlerInvocationResult.Success("HandlerA"),
            HandlerInvocationResult.Failure("HandlerB", "503")
        );

        IReadOnlyList<OutboxHandlerExecution>? captured = null;
        _repository
            .AddExecutionsAsync(
                Arg.Do<IReadOnlyList<OutboxHandlerExecution>>(x => captured = x),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert
        captured.Should().HaveCount(2);
        captured.Should().ContainSingle(e => e.HandlerName == "HandlerA" && e.Status == "Success");
        captured.Should().ContainSingle(e => e.HandlerName == "HandlerB" && e.Status == "Failure");
    }

    // ── 9–11. handler fails — final attempt → dead-letter ────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenHandlerFailsOnFinalAttempt_ShouldDeadLetterMessage()
    {
        // Arrange
        var message = CreateFinalAttemptMessage(maxRetries: 3); // AttemptNumber = 3
        GivenBatch(message);
        GivenDispatcherReturns(HandlerInvocationResult.Failure("HandlerA", "error"));

        // Act
        var output = await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert
        message.IsDeadLettered.Should().BeTrue();
        message.IsFailed.Should().BeFalse("dead-letter is a distinct terminal state from failed");
        output.DeadLettered.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerFailsOnFinalAttempt_ShouldNotAddRetry()
    {
        // Arrange
        var message = CreateFinalAttemptMessage(maxRetries: 3);
        GivenBatch(message);
        GivenDispatcherReturns(HandlerInvocationResult.Failure("HandlerA", "error"));

        // Act
        await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert
        await _repository.DidNotReceive().AddRetryAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerFailsOnFinalAttempt_ShouldStillAddExecutionRecords()
    {
        // Arrange
        var message = CreateFinalAttemptMessage(maxRetries: 3);
        GivenBatch(message);
        GivenDispatcherReturns(HandlerInvocationResult.Failure("HandlerA", "final error"));

        // Act
        await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert — execution history must be persisted even when dead-lettering
        await _repository
            .Received(1)
            .AddExecutionsAsync(
                Arg.Is<IReadOnlyList<OutboxHandlerExecution>>(e => e.Count == 1 && e[0].Status == "Failure"),
                Arg.Any<CancellationToken>()
            );
    }

    // ── 12–14. dispatcher throws (pre-handler error) ─────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenDispatcherThrowsOnNonFinalAttempt_ShouldCreateRetry()
    {
        // Arrange
        var message = CreateMessage(); // AttemptNumber = 1
        GivenBatch(message);
        _dispatcherInvoker
            .InvokeAsync(Arg.Any<IOutboxMessageDispatcher>(), Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("unknown event type"));

        // Act
        await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert
        message.IsFailed.Should().BeTrue();
        await _repository.Received(1).AddRetryAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenDispatcherThrowsOnNonFinalAttempt_ShouldAddDispatcherExecutionRecord()
    {
        // Arrange
        var message = CreateMessage();
        GivenBatch(message);
        _dispatcherInvoker
            .InvokeAsync(Arg.Any<IOutboxMessageDispatcher>(), Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("no handlers registered"));

        IReadOnlyList<OutboxHandlerExecution>? captured = null;
        _repository
            .AddExecutionsAsync(
                Arg.Do<IReadOnlyList<OutboxHandlerExecution>>(x => captured = x),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert
        captured.Should().ContainSingle();
        captured![0].HandlerName.Should().Be("Dispatcher");
        captured![0].Status.Should().Be("Failure");
        captured![0].ErrorMessage.Should().Be("no handlers registered");
    }

    [Fact]
    public async Task ExecuteAsync_WhenDispatcherThrowsOnFinalAttempt_ShouldDeadLetterMessage()
    {
        // Arrange
        var message = CreateFinalAttemptMessage(maxRetries: 3); // AttemptNumber = 3
        GivenBatch(message);
        _dispatcherInvoker
            .InvokeAsync(Arg.Any<IOutboxMessageDispatcher>(), Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("deserialization failed"));

        // Act
        var output = await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert
        message.IsDeadLettered.Should().BeTrue();
        output.DeadLettered.Should().Be(1);
        await _repository.DidNotReceive().AddRetryAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>());
    }

    // ── 15–16. batch isolation ───────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenBatchHasMultipleMessages_ShouldReturnCorrectCounts()
    {
        // Arrange — 3 messages: 2 succeed, 1 fails on non-final attempt
        var ok1 = CreateMessage();
        var ok2 = CreateMessage();
        var fail = CreateMessage();

        GivenBatch(ok1, ok2, fail);

        _dispatcherInvoker
            .InvokeAsync(
                Arg.Any<IOutboxMessageDispatcher>(),
                Arg.Is<OutboxMessage>(m => m.Id == ok1.Id || m.Id == ok2.Id),
                Arg.Any<CancellationToken>()
            )
            .Returns(new List<HandlerInvocationResult> { HandlerInvocationResult.Success("HandlerA") });

        _dispatcherInvoker
            .InvokeAsync(
                Arg.Any<IOutboxMessageDispatcher>(),
                Arg.Is<OutboxMessage>(m => m.Id == fail.Id),
                Arg.Any<CancellationToken>()
            )
            .Returns(new List<HandlerInvocationResult> { HandlerInvocationResult.Failure("HandlerA", "err") });

        // Act
        var output = await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert
        output.Processed.Should().Be(2);
        output.Failed.Should().Be(1);
        output.DeadLettered.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOneMessageThrows_ShouldStillProcessRemainingMessages()
    {
        // Arrange — second message causes a dispatcher exception
        var first = CreateMessage();
        var second = CreateMessage();
        var third = CreateMessage();

        GivenBatch(first, second, third);

        _dispatcherInvoker
            .InvokeAsync(
                Arg.Any<IOutboxMessageDispatcher>(),
                Arg.Is<OutboxMessage>(m => m.Id == second.Id),
                Arg.Any<CancellationToken>()
            )
            .ThrowsAsync(new Exception("transient failure"));

        _dispatcherInvoker
            .InvokeAsync(
                Arg.Any<IOutboxMessageDispatcher>(),
                Arg.Is<OutboxMessage>(m => m.Id != second.Id),
                Arg.Any<CancellationToken>()
            )
            .Returns(new List<HandlerInvocationResult> { HandlerInvocationResult.Success("HandlerA") });

        // Act
        var output = await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert
        first.IsProcessed.Should().BeTrue("first message must succeed regardless of second");
        third.IsProcessed.Should().BeTrue("third message must succeed regardless of second");
        output.Processed.Should().Be(2);
        output.Failed.Should().Be(1);
    }

    // ── 17–18. context hydration ─────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenMessageIsDispatched_ShouldHydrateContextWithMessageData()
    {
        // Arrange
        var message = CreateMessage();
        GivenBatch(message);
        GivenDispatcherReturns(HandlerInvocationResult.Success("HandlerA"));

        // Act
        await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert
        _contextSetter.Received(1).Set(message.TenantId, Arg.Any<string>(), message.UserId, Arg.Any<string?>());

        _contextSetter.Received(1).SetCorrelationId(message.CorrelationId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBatchHasMultipleMessages_ShouldHydrateContextForEachMessage()
    {
        // Arrange
        var messages = new[] { CreateMessage(), CreateMessage(), CreateMessage() };
        GivenBatch(messages);
        GivenDispatcherReturns(HandlerInvocationResult.Success("HandlerA"));

        // Act
        await _sut.ExecuteAsync(DefaultCommand, CancellationToken.None);

        // Assert — Set and SetCorrelationId called once per message
        _contextSetter.Received(3).Set(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<string?>());

        _contextSetter.Received(3).SetCorrelationId(Arg.Any<string>());
    }
}
