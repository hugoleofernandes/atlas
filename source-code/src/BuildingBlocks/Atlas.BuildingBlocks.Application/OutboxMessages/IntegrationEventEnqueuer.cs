using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.SharedKernel.Domain.Events;

namespace Atlas.BuildingBlocks.Application.OutboxMessages;

public sealed class IntegrationEventEnqueuer : IIntegrationEventEnqueuer
{
    private readonly IOutboxMessageBuilder _outboxMessageBuilder;
    private readonly IEnumerable<IIntegrationEventMapper> _integrationEventMappers;
    private readonly IOutboxMessageRepository _outboxMessageRepository;

    public IntegrationEventEnqueuer(
        IOutboxMessageBuilder outboxMessageBuilder,
        IEnumerable<IIntegrationEventMapper> integrationEventMappers,
        IOutboxMessageRepository outboxMessageRepository)
    {
        _outboxMessageBuilder = outboxMessageBuilder;
        _integrationEventMappers = integrationEventMappers;
        _outboxMessageRepository = outboxMessageRepository;
    }

    public async Task EnqueueAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct)
    {
        var outboxMessages = _outboxMessageBuilder
            .BuildFromIntegrationEvents(domainEvents, _integrationEventMappers)
            .ToList();

        if (outboxMessages.Count == 0)
            return;

        await _outboxMessageRepository.AddRangeAsync(outboxMessages, ct);
    }
}
