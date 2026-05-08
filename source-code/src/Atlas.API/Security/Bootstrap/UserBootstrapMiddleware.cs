using Atlas.Identity.Application.Abstractions.Tenants.Commands.ResolveAccess;
using Atlas.Identity.Application.Tenants.Commands.ResolveAccess.UserCase;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Atlas.API.Security.Bootstrap;

/// <summary>
/// Executes the application bootstrap for authenticated users.
///
/// Responsibilities:
/// - Resolve tenant access
/// - Create internal application claims
/// - Persist claims into the auth cookie
/// - Ensure bootstrap runs only once
///
/// Notes:
/// - Runs AFTER authentication
/// - Runs BEFORE authorization
/// - Safe for load balancers because state is stored in the cookie
/// </summary>
public sealed class UserBootstrapMiddleware
{
    private readonly RequestDelegate _next;

    public UserBootstrapMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
    HttpContext context,
    IResolveAccessWorkflow resolveAccessWorkflow)
    {
        //
        // ==========================================
        // ONLY AUTHENTICATED USERS
        // ==========================================
        //

        if (context.User?.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        //
        // ==========================================
        // BOOTSTRAP ALREADY COMPLETED
        // ==========================================
        //

        var alreadyBootstrapped =
            context.User.HasClaim(
                AtlasClaims.BootstrapCompleted,
                "true");

        if (alreadyBootstrapped)
        {
            await _next(context);
            return;
        }

        //
        // ==========================================
        // EXTRACT EXTERNAL CLAIMS
        // ==========================================
        //

        var oid = context.User
            .FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")
            ?.Value;

        var email = context.User
            .FindFirst("preferred_username")
            ?.Value;

        var tenantName = context.User
            .FindFirst("tenant_name")
            ?.Value;

        //
        // ==========================================
        // VALIDATE REQUIRED CLAIMS
        // ==========================================
        //

        if (string.IsNullOrWhiteSpace(oid) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(tenantName))
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsync(
                "Missing required identity claims.");

            return;
        }

        //
        // ==========================================
        // RESOLVE TENANT ACCESS
        // ==========================================
        //

        var cmd = new Command(
            tenantName,
            oid,
            email
        );

        var result = await resolveAccessWorkflow.ExecuteAsync(
            cmd,
            context.RequestAborted
        );

        //
        // ==========================================
        // ACCESS FAILED
        // ==========================================
        //

        if (!result.IsSuccess)
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            await context.Response.WriteAsync(
                result.Error ?? "Failed to resolve tenant access.");

            return;
        }

        //
        // ==========================================
        // CREATE INTERNAL CLAIMS
        // ==========================================
        //

        var value = result.Value!;

        var identity = new ClaimsIdentity("atlas");

        identity.AddClaim(new Claim(AtlasClaims.TenantId, value.TenantId.ToString()));

        identity.AddClaim(new Claim(AtlasClaims.TenantName, value.TenantName));

        identity.AddClaim(new Claim(AtlasClaims.UserId, value.UserId.ToString()));

        identity.AddClaim(new Claim(ClaimTypes.Role, value.Role));

        identity.AddClaim(new Claim(AtlasClaims.BootstrapCompleted, "true"));

        //
        // ==========================================
        // ATTACH CLAIMS TO CURRENT USER
        // ==========================================
        //

        context.User.AddIdentity(identity);

        //
        // ==========================================
        // PERSIST UPDATED COOKIE
        // ==========================================
        //

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            context.User);

        //
        // ==========================================
        // CONTINUE PIPELINE
        // ==========================================
        //

        await _next(context);
    }

}