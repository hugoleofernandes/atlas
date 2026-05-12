using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Application.IntegrationEvents;

public sealed record IntegrationEventMapping(
    IDomainEvent Event,
    OutboxEventDefinition Definition
);