namespace Atlas.OutboxWorker.Configuration;

public sealed class OutboxWorkerOptions
{
    public int BatchSize { get; set; } = 50;
    public int DegreeOfParallelism { get; set; } = Environment.ProcessorCount * 2;
    public int MaxRetries { get; set; } = 5;
    public TimeSpan LockDuration { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);
}
