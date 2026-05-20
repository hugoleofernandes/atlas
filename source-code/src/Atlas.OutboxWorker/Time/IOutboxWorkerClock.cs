namespace Atlas.Outbox.Worker.Time;

public interface IOutboxWorkerClock
{
    DateTime UtcNow { get; }
}
