using Atlas.API.Security.OIDC;
using Atlas.SharedKernel.Application;

namespace Atlas.API.Security.Tenancy;

public sealed class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolverMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context, IRequestContextSetter requestContextSetter)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantIdRaw = context.User.FindFirst(ClaimConstants.TenantId)?.Value;
            var tenantName  = context.User.FindFirst(ClaimConstants.TenantName)?.Value;
            var userIdRaw   = context.User.FindFirst(ClaimConstants.UserId)?.Value;
            var userEmail   = context.User.FindFirst(ClaimConstants.Email)?.Value;

            if (Guid.TryParse(tenantIdRaw, out var tenantId) &&
                Guid.TryParse(userIdRaw, out var userId))
            {
                requestContextSetter.Set(tenantId, tenantName!, userId, userEmail);
            }
        }

        await _next(context);
    }
}
