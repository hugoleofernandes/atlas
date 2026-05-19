namespace Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;

public interface IResolveTenantAccessCommandHandler
{
    Task<ResolveTenantAccessOutput> ExecuteAsync(ResolveTenantAccessCommand cmd, CancellationToken ct);
}
