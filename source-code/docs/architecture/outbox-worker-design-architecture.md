Outbox Worker — Architecture Design (Phase 1)
Purpose
Define the technical architecture, abstractions, and processing model of the Outbox Worker responsible for consuming OutboxMessage records and dispatching integration events in a batched, parallel, deterministic, and resilient way.

High-Level Architecture
Components
OutboxWorkerHostedService

BackgroundService that runs the processing loop.

Controls scheduling, batch execution, and shutdown.

IOutboxProcessor

Orchestrates one processing cycle:

Fetch batch

Lock messages

Dispatch in parallel

Persist state

IOutboxMessageRepository

Abstraction over persistence of OutboxMessage.

Provides batch fetch and state update operations.

IOutboxMessageDispatcher

Resolves integration event type and handler.

Deserializes payload.

Invokes IIntegrationEventHandler<TEvent>.

IIntegrationEventHandler<TEvent>

Contract for integration event handlers.

Encapsulates side effects (message bus, external APIs, etc.).

IOutboxWorkerClock

Abstraction over time (DateTime.UtcNow) for testability.

OutboxWorkerOptions

Configuration for batch size, parallelism, retries, intervals, lock duration.

Processing Strategy
Batch-Based Processing
The worker processes messages in batches, not one-by-one.

Batch fetch reduces database round-trips.

Parallel dispatch improves throughput.

Locking per message avoids concurrent processing.

Configurable batch size allows tuning per environment.

Key Abstractions
IOutboxProcessor
csharp
public interface IOutboxProcessor
{
    Task ProcessBatchAsync(CancellationToken cancellationToken);
}
Responsibilities:

Fetch pending messages via IOutboxMessageRepository.

Apply optimistic locks.

Filter messages eligible for processing.

Dispatch messages in parallel via IOutboxMessageDispatcher.

Update message state (processed/failed/dead-lettered).

IOutboxMessageRepository
csharp
public interface IOutboxMessageRepository
{
    Task<IReadOnlyList<OutboxMessage>> GetPendingBatchAsync(
        int batchSize,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
Responsibilities:

Query pending OutboxMessage records.

Respect basic eligibility filters (ProcessedOn, DeadLetteredOn, LockedUntil).

Persist state changes made on OutboxMessage instances.

IOutboxMessageDispatcher
csharp
public interface IOutboxMessageDispatcher
{
    Task DispatchAsync(OutboxMessage message, CancellationToken cancellationToken);
}
Responsibilities:

Resolve .NET type from message.Type.

Deserialize message.Payload into the integration event instance.

Resolve the corresponding IIntegrationEventHandler<TEvent> from DI.

Invoke handler.

Let exceptions bubble to the processor (for retry/dead-letter logic).

IIntegrationEventHandler<TEvent>
csharp
public interface IIntegrationEventHandler<in TEvent>
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
Responsibilities:

Implement side effects for a specific integration event.

Be idempotent.

Not depend directly on Domain/Application layers.

OutboxWorkerOptions
csharp
public sealed class OutboxWorkerOptions
{
    public int BatchSize { get; set; } = 50;
    public int DegreeOfParallelism { get; set; } = Environment.ProcessorCount * 2;
    public int MaxRetries { get; set; } = 5;
    public TimeSpan LockDuration { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);
}
Processing Flow (Detailed)
One Processing Cycle
Fetch batch

Apply lock

Filter by retry rules

Dispatch in parallel

Update state

Save changes

Wait PollInterval

Repeat

Locking & Concurrency
Locking is per message using TryLock.

workerId is a unique identifier per worker instance.

Multiple workers can run in parallel safely:

They may fetch overlapping sets.

Only one will lock each message.

Others skip locked messages.

Hosted Service
csharp
public sealed class OutboxWorkerHostedService : BackgroundService
{
    private readonly IOutboxProcessor _processor;
    private readonly OutboxWorkerOptions _options;

    public OutboxWorkerHostedService(
        IOutboxProcessor processor,
        IOptions<OutboxWorkerOptions> options)
    {
        _processor = processor;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _processor.ProcessBatchAsync(stoppingToken);
            await Task.Delay(_options.PollInterval, stoppingToken);
        }
    }
}
Type Resolution & Deserialization
OutboxMessage.Type stores the full CLR type name of the integration event.

Payload is deserialized using System.Text.Json.

Optional resolver:

csharp
public interface IIntegrationEventTypeResolver
{
    Type? Resolve(string typeName);
}
Extensibility
To add a new integration event:

Create event class (e.g., UserInvitedIntegrationEvent).

Create handler implementing IIntegrationEventHandler<UserInvitedIntegrationEvent>.

Register handler in DI.

Ensure Application Layer writes OutboxMessage with:

Name = business name (optional).

Type = full CLR type name.

Payload = serialized event.

Module = bounded context/module name.

No changes to the worker core.

Testing Hooks
IOutboxWorkerClock for deterministic time.

Mock IOutboxMessageRepository for processor tests.

Mock IOutboxMessageDispatcher for processor tests.

Use fake handlers for dispatcher tests.

HostedService tests com PollInterval curto e cancelamento controlado.

Folder Structure (Suggested)
Código
/src/Atlas.OutboxWorker
    /Configuration
        OutboxWorkerOptions.cs
    /Processing
        IOutboxProcessor.cs
        OutboxProcessor.cs
    /Dispatching
        IOutboxMessageDispatcher.cs
        OutboxMessageDispatcher.cs
        IIntegrationEventHandler.cs
        IIntegrationEventTypeResolver.cs
    /Infrastructure
        IOutboxMessageRepository.cs
        EfCoreOutboxMessageRepository.cs
    /Hosting
        OutboxWorkerHostedService.cs
    /Time
        IOutboxWorkerClock.cs
        SystemOutboxWorkerClock.cs
Mermaid Diagram
mermaid
flowchart TD

    A[OutboxWorkerHostedService] --> B[OutboxProcessor.ProcessBatchAsync]
    B --> C[GetPendingBatchAsync]
    C --> D[Apply TryLock per message]
    D --> E[Filter CanBeProcessed]
    E --> F[Parallel Dispatch]
    F --> G[MarkAsProcessed / MarkAsFailed / MarkAsDeadLettered]
    G --> H[SaveChangesAsync]
    H --> I[Wait PollInterval]
    I --> B
Summary
Batch-based

Parallel

Lock-safe

Stateless

Extensible

Deterministic

Aligned with DDD + Clean Architecture

Se ainda assim o Edge estiver a cortar, a gente pode dividir em duas partes menores (Parte 1 / Parte 2) para copiar em dois blocos.