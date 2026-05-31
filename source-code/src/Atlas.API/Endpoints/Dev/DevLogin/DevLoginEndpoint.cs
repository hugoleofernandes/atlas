using System.Security.Claims;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Commands.DevLogin;
using Atlas.Platform.Application.Queries.Tenants.GetTenantByName;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using AtlasClaims = Atlas.BuildingBlocks.AspNetCore.Security.AtlasClaims;

namespace Atlas.API.Endpoints.Dev.DevLogin;

/// <summary>
/// Development-only endpoint — creates a session cookie without going through Entra ID.
/// Orchestrates Platform (tenant resolution) + Identity (user access resolution).
/// Returns 404 in any non-Development environment.
/// </summary>
[AllowAnonymous]
public sealed class DevLoginEndpoint(
    IGetTenantByNameQueryHandler getTenantHandler,
    IDevLoginCommandHandler devLoginHandler,
    IHandlerInvoker invoker,
    IHostEnvironment env
) : AtlasEndpoint<DevLoginRequest, DevLoginResponse>
{
    public override void Configure()
    {
        Post("dev/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DevLoginRequest req, CancellationToken ct)
    {
        if (!env.IsDevelopment())
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Step 1 — Platform: resolve tenant by name
        var tenantResult = await invoker.InvokeAsync(getTenantHandler, new GetTenantByNameQuery(req.TenantName), ct);

        if (!tenantResult.IsSuccess)
        {
            await SendErrorAsync(tenantResult.ErrorDefinition!);
            return;
        }

        var tenant = tenantResult.Value!;

        // Step 2 — Identity: resolve user access
        var cmd = new DevLoginCommand(tenant.TenantId, tenant.TenantName, req.Email);
        var result = await invoker.InvokeAsync(devLoginHandler, cmd, ct);

        if (!result.IsSuccess)
        {
            await SendErrorAsync(result.ErrorDefinition!);
            return;
        }

        var value = result.Value!;

        var identity = new ClaimsIdentity("dev");
        identity.AddClaim(new Claim(AtlasClaims.TenantId, value.TenantId.ToString()));
        identity.AddClaim(new Claim(AtlasClaims.TenantName, value.TenantName));
        identity.AddClaim(new Claim(AtlasClaims.UserId, value.UserId.ToString()));
        identity.AddClaim(new Claim(AtlasClaims.UserEmail, req.Email));
        identity.AddClaim(new Claim(AtlasClaims.RoleId, value.RoleId.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Role, value.RoleName));
        identity.AddClaim(new Claim(AtlasClaims.BootstrapCompleted, "true"));

        foreach (var permission in value.Permissions)
            identity.AddClaim(new Claim(AtlasClaims.Permission, permission));

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        await Send.OkAsync(
            new DevLoginResponse(
                TenantId: value.TenantId,
                TenantName: value.TenantName,
                UserId: value.UserId,
                Email: req.Email,
                RoleName: value.RoleName,
                Permissions: value.Permissions
            ),
            ct
        );
    }
}
