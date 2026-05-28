using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.API.Configs;
using Atlas.Identity.API.Endpoints.Auth;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Atlas.Identity.API.Endpoints.Auth.Login;

/// <summary>
/// Initiates the OIDC login flow for the requested tenant.
/// Validates that the tenant is configured, then issues a Challenge to the
/// corresponding OIDC scheme, which redirects the browser to the IdP.
/// </summary>
public sealed class LoginEndpoint(
    IConfiguration           config,
    IOptions<FrontendConfig> frontOptions
) : AtlasEndpoint<LoginRequest, EmptyResponse>
{
    public override void Configure()
    {
        Get("auth/login");
        AllowAnonymous();
        Description(d => d.Produces(302));
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Tenant))
        {
            await SendErrorAsync(AuthErrors.Tenant.NameRequired);
            return;
        }

        var tenants = config.GetSection("Tenants")
            .GetChildren()
            .Select(c => c.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!tenants.Contains(req.Tenant))
        {
            await SendErrorAsync(AuthErrors.Tenant.Invalid);
            return;
        }

        var props = new AuthenticationProperties
        {
            RedirectUri = $"{frontOptions.Value.BaseUrl}/admin/home"
        };

        await HttpContext.ChallengeAsync(req.Tenant, props);
    }
}
