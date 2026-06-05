//using System.Collections.Concurrent;
//using Atlas.OutboxWorker.Configuration;
//using Atlas.OutboxWorker.Dispatching;
//using Atlas.OutboxWorker.Processing;
//using Atlas.SharedKernel.Application.OutboxMessages;
//using FluentAssertions;
//using Microsoft.Extensions.Logging.Abstractions;
//using Microsoft.Extensions.Options;
//using NSubstitute;
//using NSubstitute.ExceptionExtensions;

//namespace Atlas.OutboxWorker.Tests.Processing;

//public class OutboxProcessorTests
//{
//    // ============================================================
//    // SETUP
//    // ============================================================

//    private readonly IOutboxWorkerRepository _repository;
//    private readonly IOutboxMessageDispatcher _dispatcher;
//    private readonly OutboxWorkerOptions _options;

//    public OutboxProcessorTests()
//    {
//        _repository = Substitute.For<IOutboxWorkerRepository>();
//        _dispatcher = Substitute.For<IOutboxMessageDispatcher>();

//        _options = new OutboxWorkerOptions
//        {
//            BatchSize = 10,
//            DegreeOfParallelism = 1,
//            MaxRetries = 3,
//            LockDuration = TimeSpan.FromSeconds(30),
//            PollInterval = TimeSpan.FromSeconds(5)
//        };
//    }

//    private OutboxProcessor CreateProcessor(IOutboxWorkerRepository? repo = null, IOutboxMessageDispatcher? dispatcher = null) =>
//        new(
//            [repo ?? _repository],
//            dispatcher ?? _dispatcher,
//            Options.Create(_options),
//            NullLogger<OutboxProcessor>.Instance);

//    // ============================================================
//    // 1. DISPATCH SUCCESS → PROCESSED
//    // ============================================================

//    [Fact]
//    public async Task ProcessBatch_ShouldMarkAsProcessed_WhenDispatchSucceeds()
//    {
//        var message = CreateMessage();
//        SetupRepository(message);

//        await CreateProcessor().ProcessBatchAsync(default);

//        message.IsProcessed.Should().BeTrue();
//        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
//    }

//    // ============================================================
//    // 2. DISPATCH FAILURE → FAILED
//    // ============================================================

//    [Fact]
//    public async Task ProcessBatch_ShouldMarkAsFailed_WhenDispatchThrows()
//    {
//        var message = CreateMessage();
//        SetupRepository(message);
//        _dispatcher.DispatchAsync(message, Arg.Any<CancellationToken>())
//            .ThrowsAsync(new Exception("handler error"));

//        await CreateProcessor().ProcessBatchAsync(default);

//        message.IsProcessed.Should().BeFalse();
//        message.RetryCount.Should().Be(1);
//    }

//    // ============================================================
//    // 3. RETRIES EXCEEDED → DEAD-LETTERED NO PICKUP
//    // ============================================================

//    [Fact]
//    public async Task ProcessBatch_ShouldMarkAsDeadLettered_WhenRetriesExceededAtPickup()
//    {
//        var message = CreateMessageWithRetries(_options.MaxRetries);
//        SetupRepository(message);

//        await CreateProcessor().ProcessBatchAsync(default);

//        message.IsDeadLettered.Should().BeTrue();
//        await _dispatcher.DidNotReceive().DispatchAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>());
//    }

//    // ============================================================
//    // 4. RETRIES EXCEEDED DURANTE DISPATCH → DEAD-LETTERED
//    // ============================================================

//    [Fact]
//    public async Task ProcessBatch_ShouldMarkAsDeadLettered_WhenLastRetryFails()
//    {
//        // MaxRetries - 1 tentativas já feitas, esta é a última
//        var message = CreateMessageWithRetries(_options.MaxRetries - 1);
//        SetupRepository(message);
//        _dispatcher.DispatchAsync(message, Arg.Any<CancellationToken>())
//            .ThrowsAsync(new Exception("final error"));

//        await CreateProcessor().ProcessBatchAsync(default);

//        message.IsDeadLettered.Should().BeTrue();
//        message.RetryCount.Should().Be(_options.MaxRetries);
//    }

//    // ============================================================
//    // 5. SAVE CHANGES
//    // ============================================================

//    [Fact]
//    public async Task ProcessBatch_ShouldCallSaveChanges_AfterProcessing()
//    {
//        SetupRepository(CreateMessage());

//        await CreateProcessor().ProcessBatchAsync(default);

//        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
//    }

//    [Fact]
//    public async Task ProcessBatch_ShouldNotCallDispatch_WhenNoPendingMessages()
//    {
//        _repository.GetPendingBatchAsync(Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
//            .Returns(Array.Empty<OutboxMessage>());

//        await CreateProcessor().ProcessBatchAsync(default);

//        await _dispatcher.DidNotReceive().DispatchAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>());
//    }

//    // ============================================================
//    // 6. CONCORRÊNCIA — dois workers não processam a mesma mensagem
//    // ============================================================

//    [Fact]
//    public async Task TwoWorkers_ShouldProcessEachMessageExactlyOnce_WhenRunningConcurrently()
//    {
//        // Arrange — 5 mensagens, 2 processors compartilhando o mesmo repositório atômico
//        var messages = Enumerable.Range(0, 5).Select(_ => CreateMessage()).ToList();
//        var atomicRepo = new AtomicFakeRepository(messages, _options.BatchSize);

//        var dispatchLog = new ConcurrentBag<Guid>();
//        var countingDispatcher = Substitute.For<IOutboxMessageDispatcher>();
//        countingDispatcher
//            .DispatchAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>())
//            .Returns(ci =>
//            {
//                dispatchLog.Add(ci.Arg<OutboxMessage>().Id);
//                return Task.CompletedTask;
//            });

//        var processor1 = CreateProcessor(atomicRepo, countingDispatcher);
//        var processor2 = CreateProcessor(atomicRepo, countingDispatcher);

//        // Act — ambos rodam ao mesmo tempo
//        await Task.WhenAll(
//            processor1.ProcessBatchAsync(default),
//            processor2.ProcessBatchAsync(default));

//        // Assert — cada mensagem dispatched exatamente uma vez
//        dispatchLog.Should().HaveCount(5);
//        dispatchLog.Distinct().Should().HaveCount(5, "nenhuma mensagem deve ser processada duas vezes");
//    }

//    // ============================================================
//    // 7. LOCK EXPIRADO — outro worker assume após LockDuration
//    // ============================================================

//    [Fact]
//    public async Task ProcessBatch_ShouldPickupMessage_WhenPreviousLockExpired()
//    {
//        // Arrange — mensagem com lock expirado (outro worker morreu)
//        var message = CreateMessageWithExpiredLock();
//        var atomicRepo = new AtomicFakeRepository([message], _options.BatchSize);

//        await CreateProcessor(atomicRepo).ProcessBatchAsync(default);

//        message.IsProcessed.Should().BeTrue();
//    }

//    // ============================================================
//    // 8. RESILÊNCIA DO LOOP — handler throw não mata o worker
//    // ============================================================

//    [Fact]
//    public async Task HostedService_ShouldContinueLoop_AfterProcessorThrows()
//    {
//        // Arrange — processor falha na 1ª chamada, sucede na 2ª
//        var callCount = 0;
//        var fakeProcessor = Substitute.For<IOutboxProcessor>();
//        fakeProcessor
//            .ProcessBatchAsync(Arg.Any<CancellationToken>())
//            .Returns(_ =>
//            {
//                callCount++;
//                if (callCount == 1)
//                    throw new Exception("transient error");
//                return Task.CompletedTask;
//            });

//        using var cts = new CancellationTokenSource();

//        var hostedService = new Hosting.OutboxWorkerHostedService(
//            CreateScopeFactoryFor(fakeProcessor),
//            Options.Create(new OutboxWorkerOptions { PollInterval = TimeSpan.FromMilliseconds(10) }),
//            NullLogger<Hosting.OutboxWorkerHostedService>.Instance);

//        // Act — roda por tempo suficiente para 2+ ciclos
//        var execution = hostedService.StartAsync(cts.Token);
//        await Task.Delay(100);
//        await cts.CancelAsync();
//        await execution;

//        // Assert — processou mais de uma vez (loop não parou na exceção)
//        callCount.Should().BeGreaterThan(1);
//    }

//    // ============================================================
//    // 9. CANCELLATION — shutdown graceful
//    // ============================================================

//    [Fact]
//    public async Task HostedService_ShouldStop_WhenCancellationRequested()
//    {
//        var fakeProcessor = Substitute.For<IOutboxProcessor>();
//        fakeProcessor.ProcessBatchAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

//        using var cts = new CancellationTokenSource();

//        var hostedService = new Hosting.OutboxWorkerHostedService(
//            CreateScopeFactoryFor(fakeProcessor),
//            Options.Create(new OutboxWorkerOptions { PollInterval = TimeSpan.FromMilliseconds(10) }),
//            NullLogger<Hosting.OutboxWorkerHostedService>.Instance);

//        var execution = hostedService.StartAsync(cts.Token);
//        await Task.Delay(50);
//        await cts.CancelAsync();

//        // Deve concluir sem lançar exceção
//        var act = async () => await execution;
//        await act.Should().NotThrowAsync();
//    }

//    // ============================================================
//    // HELPERS
//    // ============================================================

//    private void SetupRepository(params OutboxMessage[] messages) =>
//        _repository.GetPendingBatchAsync(Arg.Any<int>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
//            .Returns(messages);

//    private static OutboxMessage CreateMessage() =>
//        new("test.event", "Atlas.Tests.FakeEvent", "{\"value\": 1}",
//            tenantId: Guid.NewGuid(), userId: null, correlationId: null, module: "tests");

//    private static OutboxMessage CreateMessageWithRetries(int retries)
//    {
//        var message = CreateMessage();
//        for (var i = 0; i < retries; i++)
//            message.MarkAsFailed("previous error");
//        return message;
//    }

//    private static OutboxMessage CreateMessageWithExpiredLock()
//    {
//        var message = CreateMessage();
//        // Simula lock de outro worker que já expirou
//        message.TryLock(Guid.NewGuid(), TimeSpan.FromMilliseconds(1));
//        Thread.Sleep(5); // aguarda expirar
//        return message;
//    }

//    private static Microsoft.Extensions.DependencyInjection.IServiceScopeFactory CreateScopeFactoryFor(IOutboxProcessor processor)
//    {
//        var scope = Substitute.For<Microsoft.Extensions.DependencyInjection.IServiceScope>();
//        var sp = Substitute.For<IServiceProvider>();
//        sp.GetService(typeof(IOutboxProcessor)).Returns(processor);
//        scope.ServiceProvider.Returns(sp);

//        var factory = Substitute.For<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
//        factory.CreateScope().Returns(scope);
//        return factory;
//    }
//}

//// ============================================================
//// ATOMIC FAKE REPOSITORY
//// Simula o comportamento de FOR UPDATE SKIP LOCKED:
//// aquisições concorrentes são serializadas via SemaphoreSlim,
//// garantindo que dois workers nunca recebam a mesma mensagem.
//// ============================================================

//internal sealed class AtomicFakeRepository : IOutboxWorkerRepository
//{
//    private readonly List<OutboxMessage> _store;
//    private readonly int _batchSize;
//    private readonly SemaphoreSlim _lock = new(1, 1);

//    public AtomicFakeRepository(IEnumerable<OutboxMessage> messages, int batchSize)
//    {
//        _store = messages.ToList();
//        _batchSize = batchSize;
//    }

//    public async Task<IReadOnlyList<OutboxMessage>> GetPendingBatchAsync(
//        int batchSize, TimeSpan lockDuration, CancellationToken ct)
//    {
//        await _lock.WaitAsync(ct);
//        try
//        {
//            var batch = _store
//                .Where(m => m.ProcessedOn == null
//                         && m.DeadLetteredOn == null
//                         && !m.IsLocked())
//                .Take(batchSize)
//                .ToList();

//            foreach (var m in batch)
//                m.TryLock(Guid.NewGuid(), lockDuration);

//            return batch;
//        }
//        finally
//        {
//            _lock.Release();
//        }
//    }

//    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
//}
