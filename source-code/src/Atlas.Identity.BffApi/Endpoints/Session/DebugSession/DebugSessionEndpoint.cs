using Atlas.BuildingBlocks.FastEndpoints;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Atlas.Identity.BffApi.Endpoints.Session.DebugSession;

/// <summary>
/// Returns all claims for the authenticated user — available in Development only.
/// Returns 404 in non-development environments.
/// </summary>
public sealed class DebugSessionEndpoint(
    IWebHostEnvironment env
) : AtlasEndpoint<EmptyRequest, DebugSessionResponse>
{
    public override void Configure()
    {
        Get("bff/identity/session/debug");
        Options(x => x.RequireAuthorization());
        Description(d => d.Produces<DebugSessionResponse>());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        if (!env.IsDevelopment())
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var claims = HttpContext.User.Claims
            .Select(c => new ClaimDto(c.Type, c.Value));

        await Send.OkAsync(
            new DebugSessionResponse(HttpContext.User.Identity?.Name, claims),
            ct);
    }
}
