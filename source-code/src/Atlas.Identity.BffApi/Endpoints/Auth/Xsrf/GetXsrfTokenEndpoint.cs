using Atlas.BuildingBlocks.AspNetCore.Security.Xsrf;
using Atlas.BuildingBlocks.FastEndpoints;
using FastEndpoints;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;

namespace Atlas.Identity.BffApi.Endpoints.Auth.Xsrf;

[AllowAnonymous]
public sealed class GetXsrfTokenEndpoint(IAntiforgery antiforgery)
    : AtlasEndpoint<EmptyRequest, GetXsrfTokenResponse>
{
    public override void Configure()
    {
        Get("bff/v1/identity/auth/xsrf");
        AllowAnonymous();
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var token = antiforgery.CreateAndStoreBffXsrfToken(HttpContext);
        await Send.OkAsync(new GetXsrfTokenResponse(token), ct);
    }
}
