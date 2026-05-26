using Atlas.API.Errors;
using Atlas.API.Models.Session;
using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.BuildingBlocks.AspNetCore.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.API.Controllers.Identity;

/// <summary>
/// Exposes session-related endpoints for the BFF. These endpoints allow the frontend
/// to retrieve authenticated user data (/session/me) and debug claims during development
/// (/session/debug).
///
/// This controller is the main integration point for the frontend to verify whether
/// the authentication cookie is valid and establish the user's logged-in state.
/// </summary>
[ApiController]
[Route("session")]
public class SessionController(
    ErrorMessageLocalizer errorLocalizer,
    IHttpResultMapper resultMapper) : AtlasControllerBase(errorLocalizer, resultMapper)
{
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var email = User.FindFirst(AtlasClaims.UserEmail)?.Value;
        var userId = User.FindFirst(AtlasClaims.UserId)?.Value;

        if (string.IsNullOrWhiteSpace(email))
            return ErrorResult(AuthErrors.Claim.EmailMissing);

        var dto = new GetSessionResponse(email, email, userId ?? "");

        return Ok(dto);
    }

    [HttpGet("debug")]
    [Authorize]
    public IActionResult DebugSession([FromServices] IWebHostEnvironment env)
    {
        if (!env.IsDevelopment()) return NotFound();

        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(new { user = User.Identity?.Name, claims });
    }
}
