namespace Atlas.SharedKernel.Application.IntegrationEvents;

public interface IOutboxMappings
{
    IReadOnlyList<OutboxEventDefinition> All { get; }
}
