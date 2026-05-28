using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.API.Configs;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Atlas.Identity.API.Endpoints.Auth.Logout;

/// <summary>
/// Signs the user out of the cookie session and returns the frontend redirect URL.
/// The SPA is responsible for navigating to the returned URL.
/// </summary>
public sealed class LogoutEndpoint(
    IOptions<FrontendConfig> frontOptions
) : AtlasEndpoint<EmptyRequest, LogoutResponse>
{
    public override void Configure()
    {
        Post("auth/logout-spa");
        Options(x => x.RequireAuthorization());
        Description(d => d.Produces<LogoutResponse>());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await Send.OkAsync(new LogoutResponse($"{frontOptions.Value.BaseUrl}/"), ct);
    }
}
