namespace Atlas.BuildingBlocks.Application.InternalApiInvokers;

public sealed class InternalApiInvokerOptions
{
    public string InternalApiKey { get; set; } = string.Empty;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
