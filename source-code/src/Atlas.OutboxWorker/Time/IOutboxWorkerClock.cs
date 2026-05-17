namespace Atlas.OutboxWorker.Time;

public interface IOutboxWorkerClock
{
    DateTime UtcNow { get; }
}
