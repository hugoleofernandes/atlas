using Microsoft.AspNetCore.Authentication;

namespace Atlas.BuildingBlocks.AspNetCore.Security.InternalApi;

public sealed class InternalApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string HeaderName { get; set; } = "X-Internal-Api-Key";
    public string ApiKey { get; set; } = string.Empty;
    public string ServiceName { get; set; } = "internal-service";
}
