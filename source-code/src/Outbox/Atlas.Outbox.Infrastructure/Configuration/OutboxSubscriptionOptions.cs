namespace Atlas.Outbox.Infrastructure.Configuration;

public sealed class OutboxSubscriptionOptions
{
    public string Name { get; set; } = string.Empty;
    public string Method { get; set; } = "POST";
    public string Url { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool Enabled { get; set; } = true;
}
