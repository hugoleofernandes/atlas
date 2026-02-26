using Atlas.Application.Tenancy;

namespace Atlas.API.Security.Tenancy;

public sealed class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;

    private const string TenantIdClaim = "atlas_tenant_id";
    private const string TenantSlugClaim = "atlas_tenant_slug";

    public TenantResolverMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context, ITenantContext tenantContext)
    {
        // Só resolve tenant quando o usuário estiver autenticado
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantIdRaw = context.User.FindFirst(TenantIdClaim)?.Value;
            var tenantSlug = context.User.FindFirst(TenantSlugClaim)?.Value;

            if (Guid.TryParse(tenantIdRaw, out var tenantId))
            {
                tenantContext.Set(tenantId, tenantSlug!);
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