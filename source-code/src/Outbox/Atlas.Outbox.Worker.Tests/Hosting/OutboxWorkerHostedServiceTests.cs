using Atlas.Outbox.Application.ProcessOutbox;
using Atlas.Outbox.Contracts.Commands.ProcessOutbox;
using Atlas.Outbox.Infrastructure.Configuration;
using Atlas.Outbox.Worker.Hosting;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Handlers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Atlas.OutboxWorker.Tests.Hosting;

/// <summary>
/// Unit tests for <see cref="OutboxWorkerHostedService"/>.
///
/// Strategy:
///   - The hosted service is <c>internal</c>; access is granted via
///     <c>InternalsVisibleTo("Atlas.OutboxWorker.Tests")</c> in <c>Atlas.Outbox.Worker.csproj</c>.
///   - <see cref="IHandlerInvoker"/>, <see cref="IIdentityOutboxCommandHandler"/> and
///     <see cref="IStaffOutboxCommandHandler"/> are mocked with NSubstitute and registered
///     as singletons in a real <see cref="IServiceScopeFactory"/> — the same instances are
///     resolved across scopes, allowing Received() assertions after execution.
///   - <see cref="OutboxWorkerOptions.PollInterval"/> is set to <see cref="TimeSpan.Zero"/>
///     to eliminate real delays from the test run.
///   - Synchronisation uses <see cref="TaskCompletionSource"/> (not Thread.Sleep / timeouts)
///     to signal when the loop has completed a specific number of invocations.
/// </summary>
public sealed class OutboxWorkerHostedServiceTests
{
    // ── infrastructure ────────────────────────────────────────────────────────

    private readonly IHandlerInvoker _invoker = Substitute.For<IHandlerInvoker>();
    private readonly IIdentityOutboxCommandHandler _identityHandler = Substitute.For<IIdentityOutboxCommandHandler>();
    private readonly IStaffOutboxCommandHandler _staffHandler = Substitute.For<IStaffOutboxCommandHandler>();
    private readonly ILogger<OutboxWorkerHostedService> _logger = Substitute.For<ILogger<OutboxWorkerHostedService>>();

    private readonly OutboxWorkerOptions _options = new()
    {
        PollInterval = TimeSpan.Zero, // no real delay between cycles
        BatchSize = 10,
        MaxRetries = 3,
        LockDuration = TimeSpan.FromSeconds(30),
    };

    public OutboxWorkerHostedServiceTests()
    {
        // Default: every InvokeAsync call returns an empty-batch success result.
        _invoker
            .InvokeAsync(
                Arg.Any<IHandler<ProcessOutboxCommand, ProcessOutboxOutput>>(),
                Arg.Any<ProcessOutboxCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(Result.Ok(new ProcessOutboxOutput(0, 0, 0))));
    }

    // ── factory helpers ───────────────────────────────────────────────────────

    private OutboxWorkerHostedService CreateSut()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_invoker);
        services.AddSingleton<IIdentityOutboxCommandHandler>(_identityHandler);
        services.AddSingleton<IStaffOutboxCommandHandler>(_staffHandler);

        // Resolve IServiceScopeFactory from the built container — the same singleton
        // instances are returned across every scope the hosted service creates.
        var sp = services.BuildServiceProvider();
        var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

        return new OutboxWorkerHostedService(scopeFactory, Options.Create(_options), _logger);
    }

    // ── tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenUnhandledExceptionOccurs_ShouldLogAndContinueLoop()
    {
        // Arrange — first invocation throws; second signals that the loop resumed
        var loopResumed = new TaskCompletionSource();
        var callCount = 0;

        _invoker
            .InvokeAsync(
                Arg.Any<IHandler<ProcessOutboxCommand, ProcessOutboxOutput>>(),
                Arg.Any<ProcessOutboxCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(_ =>
            {
                if (callCount++ == 0)
                    throw new InvalidOperationException("transient db failure");

                loopResumed.TrySetResult(); // loop survived the exception and re-entered
                return Task.FromResult(Result.Ok(new ProcessOutboxOutput(0, 0, 0)));
            });

        var sut = CreateSut();

        // Act
        await sut.StartAsync(CancellationToken.None);
        await loopResumed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await sut.StopAsync(CancellationToken.None);

        // Assert — loop iterated at least twice (once for the throw, once after recovery)
        callCount.Should().BeGreaterThanOrEqualTo(2, "the worker must survive transient errors and keep polling");

        // Assert — error was logged (structured ILogger.Log low-level call)
        _logger
            .Received()
            .Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Is<Exception>(ex => ex.Message == "transient db failure"),
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    [Fact]
    public async Task ExecuteAsync_WhenStopRequested_ShouldStopGracefully()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        await sut.StartAsync(CancellationToken.None);

        // Assert — StopAsync completes without throwing; OperationCanceledException is swallowed internally
        var stopAct = async () => await sut.StopAsync(CancellationToken.None);
        await stopAct.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenRunning_ShouldInvokeIdentityAndStaffHandlerEachCycle()
    {
        // Arrange — signal after both handlers in one cycle have been invoked
        var cycleComplete = new TaskCompletionSource();
        var callCount = 0;

        _invoker
            .InvokeAsync(
                Arg.Any<IHandler<ProcessOutboxCommand, ProcessOutboxOutput>>(),
                Arg.Any<ProcessOutboxCommand>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(_ =>
            {
                if (++callCount == 2)
                    cycleComplete.TrySetResult(); // identity + staff both ran
                return Task.FromResult(Result.Ok(new ProcessOutboxOutput(0, 0, 0)));
            });

        var sut = CreateSut();

        // Act
        await sut.StartAsync(CancellationToken.None);
        await cycleComplete.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await sut.StopAsync(CancellationToken.None);

        // Assert — one call for IIdentityOutboxCommandHandler, one for IStaffOutboxCommandHandler
        await _invoker
            .Received()
            .InvokeAsync(
                Arg.Is<IHandler<ProcessOutboxCommand, ProcessOutboxOutput>>(h => h is IIdentityOutboxCommandHandler),
                Arg.Any<ProcessOutboxCommand>(),
                Arg.Any<CancellationToken>()
            );

        await _invoker
            .Received()
            .InvokeAsync(
                Arg.Is<IHandler<ProcessOutboxCommand, ProcessOutboxOutput>>(h => h is IStaffOutboxCommandHandler),
                Arg.Any<ProcessOutboxCommand>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task ExecuteAsync_WhenRunning_ShouldBuildCommandFromConfiguredOptions()
    {
        // Arrange — capture the command passed to InvokeAsync on the first invocation
        ProcessOutboxCommand? capturedCommand = null;
        var commandCaptured = new TaskCompletionSource();

        _invoker
            .InvokeAsync(
                Arg.Any<IHandler<ProcessOutboxCommand, ProcessOutboxOutput>>(),
                Arg.Do<ProcessOutboxCommand>(cmd =>
                {
                    capturedCommand = cmd;
                    commandCaptured.TrySetResult();
                }),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(Result.Ok(new ProcessOutboxOutput(0, 0, 0))));

        var sut = CreateSut();

        // Act
        await sut.StartAsync(CancellationToken.None);
        await commandCaptured.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await sut.StopAsync(CancellationToken.None);

        // Assert — command reflects the configured OutboxWorkerOptions
        capturedCommand.Should().NotBeNull();
        capturedCommand!.BatchSize.Should().Be(_options.BatchSize);
        capturedCommand.MaxRetries.Should().Be(_options.MaxRetries);
        capturedCommand.LockDuration.Should().Be(_options.LockDuration);
    }
}
