namespace Atlas.Identity.Application.Tenants.UseCases.TenantEventDispatcher;

public interface ITenantEventDispatcher
{
    Task ExecuteAsync(CancellationToken ct);
}
