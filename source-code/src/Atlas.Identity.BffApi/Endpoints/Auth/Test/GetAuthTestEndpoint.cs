using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.BffApi.Configs;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Atlas.Identity.BffApi.Endpoints.Auth.Test;

/// <summary>
/// Smoke-test endpoint — confirms the auth API is reachable and returns
/// the configured tenants and frontend URL.
/// </summary>
public sealed class GetAuthTestEndpoint(
    IConfiguration         config,
    IOptions<FrontendConfig> frontOptions
) : AtlasEndpoint<EmptyRequest, GetAuthTestResponse>
{
    public override void Configure()
    {
        Get("bff/v1/identity/auth/test");
        AllowAnonymous();
        Description(d => d.Produces<GetAuthTestResponse>());
    }

    public override Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var tenants = config.GetSection("Tenants")
            .GetChildren()
            .Select(c => c.Key)
            .ToArray();

        return Send.OkAsync(
            new GetAuthTestResponse(
                Message:         "Auth API is running.",
                Tenants:         tenants,
                FrontendBaseUrl: frontOptions.Value.BaseUrl),
            ct);
    }
}
