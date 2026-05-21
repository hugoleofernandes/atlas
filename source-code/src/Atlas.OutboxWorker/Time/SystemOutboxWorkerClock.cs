namespace Atlas.Outbox.Worker.Time;

internal sealed class SystemOutboxWorkerClock : IOutboxWorkerClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
