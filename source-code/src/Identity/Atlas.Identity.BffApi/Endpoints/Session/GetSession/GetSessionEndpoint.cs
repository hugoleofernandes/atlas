using Atlas.BuildingBlocks.AspNetCore.Security;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.BffApi.Endpoints.Auth;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Atlas.Identity.BffApi.Endpoints.Session.GetSession;

/// <summary>
/// Returns the authenticated user's session data (email, userId).
/// Used by the SPA BFF to establish the user's logged-in state.
/// </summary>
public sealed class GetSessionEndpoint : AtlasEndpoint<EmptyRequest, GetSessionResponse>
{
    public override void Configure()
    {
        Get("bff/v1/identity/session/me");
        Options(x => x.RequireAuthorization());
        Description(d => d.Produces<GetSessionResponse>());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var email  = HttpContext.User.FindFirst(AtlasClaims.UserEmail)?.Value;
        var userId = HttpContext.User.FindFirst(AtlasClaims.UserId)?.Value;

        if (string.IsNullOrWhiteSpace(email))
        {
            await SendErrorAsync(AuthErrors.Claim.EmailMissing);
            return;
        }

        await Send.OkAsync(new GetSessionResponse(email, email, userId ?? ""), ct);
    }
}
