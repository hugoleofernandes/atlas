using Atlas.SharedKernel.Application;
using Microsoft.AspNetCore.Http;

namespace Atlas.BuildingBlocks.AspNetCore.Security.Tenancy;

public sealed class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolverMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context, IRequestContextSetter requestContextSetter)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantIdRaw = context.User.FindFirst(AtlasClaims.TenantId)?.Value;
            var tenantName  = context.User.FindFirst(AtlasClaims.TenantName)?.Value;
            var userIdRaw   = context.User.FindFirst(AtlasClaims.UserId)?.Value;
            var userEmail   = context.User.FindFirst(AtlasClaims.UserEmail)?.Value;

            if (Guid.TryParse(tenantIdRaw, out var tenantId) &&
                Guid.TryParse(userIdRaw, out var userId))
            {
                requestContextSetter.Set(tenantId, tenantName!, userId, userEmail);
            }
        }

        await _next(context);
    }
}
