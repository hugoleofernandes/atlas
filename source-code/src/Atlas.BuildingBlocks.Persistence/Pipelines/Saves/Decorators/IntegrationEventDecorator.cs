using Atlas.BuildingBlocks.Persistence.DbContexts;
using Atlas.BuildingBlocks.Persistence.Pipelines.Saves.Interfaces;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.BuildingBlocks.Persistence.Pipelines.Saves.Decorators;

/// <summary>
/// Converts domain events to outbox messages and adds them to the same DbContext
/// that will be saved — guaranteeing atomicity without any extra repository dependency.
/// </summary>
internal sealed class IntegrationEventDecorator : ISavePipelineStep
{
    private readonly ISavePipelineStep _inner;
    private readonly IOutboxMessageBuilder _messageBuilder;
    private readonly IEnumerable<IIntegrationEventMapper> _mappers;

    public IntegrationEventDecorator(
        ISavePipelineStep inner,
        IOutboxMessageBuilder messageBuilder,
        IEnumerable<IIntegrationEventMapper> mappers)
    {
        _inner          = inner;
        _messageBuilder = messageBuilder;
        _mappers        = mappers;
    }

    public async Task ExecuteAsync(DbContextBase db, CancellationToken ct)
    {
        var domainEvents    = db.GetDomainEvents();
        var outboxMessages  = _messageBuilder
            .BuildFromIntegrationEvents(domainEvents, _mappers)
            .ToList();

        if (outboxMessages.Count > 0)
            await db.Set<OutboxMessage>().AddRangeAsync(outboxMessages, ct);

        await _inner.ExecuteAsync(db, ct);
    }
}
