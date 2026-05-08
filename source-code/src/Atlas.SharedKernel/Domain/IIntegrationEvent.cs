namespace Atlas.SharedKernel.Domain;

public interface IIntegrationEvent
{
    string EventName { get; }

    string Module { get; }
}
