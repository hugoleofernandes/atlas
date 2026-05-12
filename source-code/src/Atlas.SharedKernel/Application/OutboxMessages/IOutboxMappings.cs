namespace Atlas.SharedKernel.Application.OutboxMessages;

public interface IOutboxMappings
{
    IReadOnlyList<OutboxEventDefinition> All { get; }
}
