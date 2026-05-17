namespace Atlas.OutboxWorker.Time;

internal sealed class SystemOutboxWorkerClock : IOutboxWorkerClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
