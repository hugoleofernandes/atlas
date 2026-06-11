using System.Text.Json;
using Atlas.Outbox.Application.Queries.ResolveOutboxTargets;
using Atlas.Outbox.Infrastructure;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Idempotency;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Application.OutboxMessages;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Atlas.OutboxWorker.Tests.Dispatching;

/// <summary>
/// Unit tests for <see cref="OutboxMessageDispatcher"/>.
///
/// Strategy:
///   - <see cref="OutboxMessageDispatcher"/> is <c>internal</c>; access is granted via
///     <c>[assembly: InternalsVisibleTo("Atlas.OutboxWorker.Tests")]</c> in Atlas.Outbox.Infrastructure.
///   - <see cref="IIntegrationEventTypeResolver"/> is also <c>internal</c> — mocked with
///     NSubstitute using the same InternalsVisibleTo grant.
///   - <see cref="IHandlerInvoker"/> is replaced by the concrete <see cref="FakeHandlerInvoker"/>
///     to avoid NSubstitute + generic-method-via-reflection unreliability (the dispatcher calls
///     InvokeAsync&lt;TInput,TOutput&gt; through reflection, which NSubstitute cannot intercept reliably).
///   - Handlers are concrete classes (<see cref="FakeSuccessHandler"/>, <see cref="FakeFailingHandler"/>)
///     registered in a real <see cref="IServiceProvider"/> — mirrors production DI resolution.
///   - <see cref="IIdempotencyContextSetter"/> is mocked to assert per-handler context wiring.
/// </summary>
public sealed class OutboxMessageDispatcherTests
{
    // ── infrastructure ────────────────────────────────────────────────────────

    private readonly IIntegrationEventTypeResolver _typeResolver = Substitute.For<IIntegrationEventTypeResolver>();

    private readonly IIdempotencyContextSetter _idempotencyContextSetter = Substitute.For<IIdempotencyContextSetter>();

    private readonly FakeHandlerInvoker _handlerInvoker = new();

    // ── factory helpers ───────────────────────────────────────────────────────

    private OutboxMessageDispatcher CreateSut(IServiceProvider serviceProvider) =>
        new(_typeResolver, _handlerInvoker, _idempotencyContextSetter, serviceProvider);

    /// <summary>
    /// Builds a <see cref="IServiceProvider"/> with the given handler instances registered
    /// as <c>IIntegrationEventHandler&lt;FakeIntegrationEvent&gt;</c>.
    /// Passing no handlers produces a provider that returns an empty sequence — used to
    /// verify the "no handler registered" guard.
    /// </summary>
    private static IServiceProvider BuildServiceProvider(
        params IIntegrationEventHandler<FakeIntegrationEvent>[] handlers
    )
    {
        var services = new ServiceCollection();
        foreach (var handler in handlers)
            services.AddSingleton<IIntegrationEventHandler<FakeIntegrationEvent>>(handler);
        return services.BuildServiceProvider();
    }

    private static OutboxMessage CreateMessage()
    {
        var payload = JsonSerializer.Serialize(new FakeIntegrationEvent("hello"));
        return new OutboxMessage(
            name: "FakeIntegrationEvent",
            type: "FakeIntegrationEvent",
            payload: payload,
            tenantId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            userEmail: "test@atlas.com",
            correlationId: "corr-id",
            module: "tests"
        );
    }

    // ── tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DispatchAsync_WhenTypeIsKnownAndHandlerSucceeds_ShouldReturnSingleSuccessResult()
    {
        // Arrange
        var message = CreateMessage();
        _typeResolver.Resolve("FakeIntegrationEvent").Returns(typeof(FakeIntegrationEvent));
        var sut = CreateSut(BuildServiceProvider(new FakeSuccessHandler()));

        // Act
        var results = await sut.DispatchAsync(message, CancellationToken.None);

        // Assert
        results.Should().HaveCount(1);
        results[0].IsSuccess.Should().BeTrue();
        results[0].HandlerName.Should().Be(nameof(FakeSuccessHandler));
    }

    [Fact]
    public async Task DispatchAsync_WhenTwoHandlersRegistered_ShouldReturnResultForEachHandler()
    {
        // Arrange
        var message = CreateMessage();
        _typeResolver.Resolve("FakeIntegrationEvent").Returns(typeof(FakeIntegrationEvent));
        var sut = CreateSut(BuildServiceProvider(new FakeSuccessHandlerA(), new FakeSuccessHandlerB()));

        // Act
        var results = await sut.DispatchAsync(message, CancellationToken.None);

        // Assert — both handlers ran and both succeeded (fan-out)
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
        results
            .Select(r => r.HandlerName)
            .Should()
            .BeEquivalentTo([nameof(FakeSuccessHandlerA), nameof(FakeSuccessHandlerB)]);
    }

    [Fact]
    public async Task DispatchAsync_WhenOneHandlerFails_ShouldContinueAndReturnMixedResults()
    {
        // Arrange
        var message = CreateMessage();
        _typeResolver.Resolve("FakeIntegrationEvent").Returns(typeof(FakeIntegrationEvent));
        var sut = CreateSut(BuildServiceProvider(new FakeSuccessHandler(), new FakeFailingHandler()));

        // Act
        var results = await sut.DispatchAsync(message, CancellationToken.None);

        // Assert — fan-out is not aborted by one handler failing
        results.Should().HaveCount(2);
        results.Should().ContainSingle(r => r.IsSuccess);
        results.Should().ContainSingle(r => !r.IsSuccess);
        results.First(r => !r.IsSuccess).ErrorMessage.Should().Contain("simulated handler failure");
    }

    [Fact]
    public async Task DispatchAsync_WhenTypeResolverReturnsNull_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var message = CreateMessage();
        _typeResolver.Resolve("FakeIntegrationEvent").Returns((Type?)null);
        var sut = CreateSut(BuildServiceProvider());

        // Act
        var act = () => sut.DispatchAsync(message, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*FakeIntegrationEvent*");
    }

    [Fact]
    public async Task DispatchAsync_WhenNoHandlerIsRegistered_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var message = CreateMessage();
        _typeResolver.Resolve("FakeIntegrationEvent").Returns(typeof(FakeIntegrationEvent));
        var sut = CreateSut(BuildServiceProvider()); // empty provider — no handlers

        // Act
        var act = () => sut.DispatchAsync(message, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*FakeIntegrationEvent*");
    }

    [Fact]
    public async Task DispatchAsync_WhenHandlerRuns_ShouldSetIdempotencyContextWithMessageKeyAndHandlerName()
    {
        // Arrange
        var message = CreateMessage();
        _typeResolver.Resolve("FakeIntegrationEvent").Returns(typeof(FakeIntegrationEvent));
        var sut = CreateSut(BuildServiceProvider(new FakeSuccessHandler()));

        // Act
        await sut.DispatchAsync(message, CancellationToken.None);

        // Assert — idempotency key is stable (IdempotencyKey == Id on the first attempt)
        _idempotencyContextSetter.Received(1).Set(message.IdempotencyKey, nameof(FakeSuccessHandler));
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// Test support types
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>Minimal integration event used as the payload in all dispatcher tests.</summary>
internal record FakeIntegrationEvent(string Value);

/// <summary>Handler that completes successfully. Used for the happy path and multi-handler fan-out.</summary>
internal sealed class FakeSuccessHandler : IIntegrationEventHandler<FakeIntegrationEvent>
{
    public Task HandleAsync(FakeIntegrationEvent @event, CancellationToken ct) => Task.CompletedTask;
}

/// <summary>Two distinct success handlers to verify the two-handler fan-out scenario.</summary>
internal sealed class FakeSuccessHandlerA : IIntegrationEventHandler<FakeIntegrationEvent>
{
    public Task HandleAsync(FakeIntegrationEvent @event, CancellationToken ct) => Task.CompletedTask;
}

internal sealed class FakeSuccessHandlerB : IIntegrationEventHandler<FakeIntegrationEvent>
{
    public Task HandleAsync(FakeIntegrationEvent @event, CancellationToken ct) => Task.CompletedTask;
}

/// <summary>
/// Handler that always throws. Verifies that the dispatcher catches per-handler exceptions
/// and continues executing the remaining handlers (fault isolation).
/// The exception propagates through the DIM bridge (ExecuteAsync → HandleAsync) as a
/// faulted Task, which the dispatcher's per-handler catch records as a failure result.
/// </summary>
internal sealed class FakeFailingHandler : IIntegrationEventHandler<FakeIntegrationEvent>
{
    public Task HandleAsync(FakeIntegrationEvent @event, CancellationToken ct) =>
        throw new InvalidOperationException("simulated handler failure");
}

/// <summary>
/// Concrete <see cref="IHandlerInvoker"/> that calls <c>handler.ExecuteAsync</c> directly
/// and wraps the output in <c>Result.Ok</c>.
///
/// Using a concrete class (instead of NSubstitute) avoids the known unreliability of
/// NSubstitute intercepting open generic methods called via reflection — which is exactly
/// how <see cref="OutboxMessageDispatcher"/> dispatches to <c>InvokeAsync&lt;TInput, TOutput&gt;</c>.
///
/// On exception → propagates the faulted Task so the dispatcher's per-handler catch fires,
/// exactly as it would through the real decorator pipeline.
/// </summary>
internal sealed class FakeHandlerInvoker : IHandlerInvoker
{
    public async Task<Result<TOutput>> InvokeAsync<TInput, TOutput>(
        IHandler<TInput, TOutput> handler,
        TInput input,
        CancellationToken ct
    )
    {
        var output = await handler.ExecuteAsync(input, ct);
        return Result.Ok(output);
    }
}
