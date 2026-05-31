using System.Security.Claims;
using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.BuildingBlocks.AspNetCore.Observability;
using Atlas.BuildingBlocks.AspNetCore.Security;
using Atlas.Identity.API.Endpoints.Auth;
using Atlas.Identity.Application.Commands.ResolveTenantAccess;
using Atlas.Platform.Application.Queries.Tenants.GetTenantByName;
using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Atlas.API.Security.Bootstrap;

/// <summary>
/// Executes the application bootstrap for authenticated users.
///
/// Responsibilities:
/// - Resolve tenant info from Platform (step 1)
/// - Resolve user access from Identity (step 2)
/// - Create internal application claims (tenantId, userId, roleId, permissions)
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
        IGetTenantByNameQueryHandler     getTenantHandler,
        IResolveTenantAccessCommandHandler resolveAccessHandler,
        IHandlerInvoker invoker,
        IErrorMessageLocalizer errorLocalizer
    )
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

        var alreadyBootstrapped = context.User.HasClaim(AtlasClaims.BootstrapCompleted, "true");

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

        var oid = context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

        var email = context.User.FindFirst("preferred_username")?.Value;

        var tenantName = context.User.FindFirst("tenant_name")?.Value;

        //
        // ==========================================
        // VALIDATE REQUIRED CLAIMS
        // ==========================================
        //

        if (string.IsNullOrWhiteSpace(oid) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(tenantName))
        {
            await WriteProblemAsync(context, AuthErrors.Claim.IdentityMissing, errorLocalizer);
            return;
        }

        //
        // ==========================================
        // STEP 1 — RESOLVE TENANT (Platform)
        // ==========================================
        //

        var tenantResult = await invoker.InvokeAsync(
            getTenantHandler,
            new GetTenantByNameQuery(tenantName),
            context.RequestAborted);

        if (!tenantResult.IsSuccess)
        {
            await WriteProblemAsync(context, tenantResult.ErrorDefinition!, errorLocalizer);
            return;
        }

        var tenant = tenantResult.Value!;

        //
        // ==========================================
        // STEP 2 — RESOLVE USER ACCESS (Identity)
        // ==========================================
        //

        var cmd = new ResolveTenantAccessCommand(tenant.TenantId, tenant.TenantName, oid, email);

        var result = await invoker.InvokeAsync(resolveAccessHandler, cmd, context.RequestAborted);

        //
        // ==========================================
        // ACCESS FAILED
        // ==========================================
        //

        if (!result.IsSuccess)
        {
            var error = result.ErrorDefinition!;
            await WriteProblemAsync(context, error, errorLocalizer);
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
        identity.AddClaim(new Claim(AtlasClaims.UserEmail, email));
        identity.AddClaim(new Claim(AtlasClaims.RoleId, value.RoleId.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Role, value.RoleName));
        identity.AddClaim(new Claim(AtlasClaims.BootstrapCompleted, "true"));

        // One claim per permission — authorization handler checks HasClaim(type, value)
        foreach (var permission in value.Permissions)
            identity.AddClaim(new Claim(AtlasClaims.Permission, permission));

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

        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, context.User);

        //
        // ==========================================
        // CONTINUE PIPELINE
        // ==========================================
        //

        await _next(context);
    }

    private static int MapCategory(ErrorCategory category) =>
        category switch
        {
            ErrorCategory.Validation => StatusCodes.Status400BadRequest,
            ErrorCategory.Business => StatusCodes.Status422UnprocessableEntity,
            ErrorCategory.Conflict => StatusCodes.Status409Conflict,
            ErrorCategory.NotFound => StatusCodes.Status404NotFound,
            ErrorCategory.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError,
        };

    private static async Task WriteProblemAsync(
        HttpContext context,
        ErrorDefinition error,
        IErrorMessageLocalizer localizer
    )
    {
        var status = MapCategory(error.Category);

        var problem = new ApiProblemDetails
        {
            Title = localizer.Localize(error),
            Status = status,
            Type = $"https://docs.atlas/errors/{error.Code}",
        };

        problem.AddMetadata(error.Code, CorrelationIdMiddleware.Get(context), TraceContextHelper.GetTraceId());

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem);
    }
}
