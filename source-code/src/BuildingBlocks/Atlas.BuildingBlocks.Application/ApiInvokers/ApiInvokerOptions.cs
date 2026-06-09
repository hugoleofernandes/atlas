namespace Atlas.BuildingBlocks.Application.ApiInvokers;

public sealed class ApiInvokerOptions
{
    public string InternalApiKey { get; set; } = string.Empty;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
