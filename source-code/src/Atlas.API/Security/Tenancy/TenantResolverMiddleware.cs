using Atlas.API.Security.OIDC;
using Atlas.SharedKernel.Application;

namespace Atlas.API.Security.Tenancy;

public sealed class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;


    public TenantResolverMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context, IRequestContext requestContext)
    {
        // Só resolve tenant quando o usuário estiver autenticado
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantIdRaw = context.User.FindFirst(ClaimConstants.TenantId)?.Value;
            var tenantSlug = context.User.FindFirst(ClaimConstants.TenantSlug)?.Value;
            var userIdRaw = context.User.FindFirst(ClaimConstants.UserId)?.Value;

            if (Guid.TryParse(tenantIdRaw, out var tenantId) &&
                Guid.TryParse(userIdRaw, out var userId))
            {
                requestContext.Set(tenantId, tenantSlug!, userId);
            }
            //else 
            //{
            //    // Se isso acontecer, OnTokenValidated não adicionou as claims,
            //    // ou o cookie/claims foram emitidos incompletos.
            //    // Melhor falhar cedo para não vazar dados.
            //    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //    await context.Response.WriteAsync("Tenant context missing.");
            //    return;
            //}
        }

        await _next(context);
    }
}