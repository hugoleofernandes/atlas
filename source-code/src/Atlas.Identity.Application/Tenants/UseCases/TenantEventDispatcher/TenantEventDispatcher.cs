using Atlas.Identity.Application.OutboxMessages.Mappings;
using Atlas.Identity.Domain.Entities.Tenants.Events;
using Atlas.SharedKernel.Application.Events;
using Atlas.SharedKernel.Application.IntegrationEvents;

namespace Atlas.Identity.Application.Tenants.UseCases.TenantEventDispatcher;

public sealed class TenantEventDispatcher : ITenantEventDispatcher
{
    private readonly IDomainEventCollector _domainEventCollector;
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly IOutboxMessageFactory _outboxMessageFactory;
    private readonly IIntegrationEventRegistry _integrationEventRegistry;
    private readonly ITenantOutboxMappings _tenantOutboxMappings;

    public TenantEventDispatcher(IDomainEventCollector domainEventCollector, IOutboxMessageRepository outboxMessageRepository, 
            IOutboxMessageFactory outboxMessageFactory, IIntegrationEventRegistry integrationEventRegistry, ITenantOutboxMappings tenantOutboxMappings)
    {
        _domainEventCollector = domainEventCollector;
        _outboxMessageRepository = outboxMessageRepository;
        _outboxMessageFactory = outboxMessageFactory;
        _integrationEventRegistry = integrationEventRegistry;
        _tenantOutboxMappings = tenantOutboxMappings;
    }


    public async Task ExecuteAsync(CancellationToken ct)
    {
        var domainEvents = _domainEventCollector.GetAll();

        var MappedIntegrationEvents = _integrationEventRegistry.ResolveAll(domainEvents, _tenantOutboxMappings).ToList();

        var userCreatedEvent = MappedIntegrationEvents.GetEvents<UserCreatedFromInvitationDomainEvent>().FirstOrDefault();

        if (userCreatedEvent is not null)
            await _outboxMessageRepository.AddAsync(_outboxMessageFactory.Create(userCreatedEvent.Event, userCreatedEvent.Definition)!, ct);

        _domainEventCollector.Clear();
    }
}