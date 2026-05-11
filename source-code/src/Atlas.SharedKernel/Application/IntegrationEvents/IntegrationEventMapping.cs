using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Application.IntegrationEvents;

public sealed record IntegrationEventMapping(
    DomainEvent Event,
    OutboxEventDefinition Definition
);