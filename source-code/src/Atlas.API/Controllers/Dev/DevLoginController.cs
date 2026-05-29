using Atlas.SharedKernel.Application.Handlers;
using Atlas.BuildingBlocks.AspNetCore.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Atlas.Identity.Application.Commands.ResolveTenantAccess;

namespace Atlas.API.Controllers.Dev;

/// <summary>
/// Development-only endpoint that creates a session cookie without going through Entra ID.
/// Returns 404 in any non-Development environment.
/// </summary>
[ApiController]
[Route("dev")]
[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class DevLoginController(
    IResolveTenantAccessCommandHandler resolveAccessHandler,
    IHandlerInvoker invoker,
    IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] DevLoginRequest request, CancellationToken ct)
    {
        if (!env.IsDevelopment())
            return NotFound();

        // Deterministic fake OID: same email always maps to the same user in the DB
        var fakeOid = new Guid(MD5.HashData(Encoding.UTF8.GetBytes(request.Email))).ToString();

        var cmd    = new ResolveTenantAccessCommand(request.TenantName, fakeOid, request.Email);
        var result = await invoker.InvokeAsync(resolveAccessHandler, cmd, ct);

        if (!result.IsSuccess)
            return Problem(result.ErrorDefinition!.FallbackMessage, statusCode: StatusCodes.Status400BadRequest);

        var value = result.Value!;

        var identity = new ClaimsIdentity("dev");
        identity.AddClaim(new Claim(AtlasClaims.TenantId,          value.TenantId.ToString()));
        identity.AddClaim(new Claim(AtlasClaims.TenantName,         value.TenantName));
        identity.AddClaim(new Claim(AtlasClaims.UserId,             value.UserId.ToString()));
        identity.AddClaim(new Claim(AtlasClaims.UserEmail,           request.Email));
        identity.AddClaim(new Claim(AtlasClaims.RoleId,             value.RoleId.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Role,                 value.RoleName));
        identity.AddClaim(new Claim(AtlasClaims.BootstrapCompleted, "true"));

        foreach (var permission in value.Permissions)
            identity.AddClaim(new Claim(AtlasClaims.Permission, permission));

        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Ok(new DevLoginResponse(
            value.TenantId,
            value.TenantName,
            value.UserId,
            request.Email,
            value.RoleName,
            value.Permissions));
    }
}

public sealed record DevLoginRequest(string TenantName, string Email);

public sealed record DevLoginResponse(
    Guid TenantId,
    string TenantName,
    Guid UserId,
    string Email,
    string RoleName,
    IReadOnlyList<string> Permissions
);
