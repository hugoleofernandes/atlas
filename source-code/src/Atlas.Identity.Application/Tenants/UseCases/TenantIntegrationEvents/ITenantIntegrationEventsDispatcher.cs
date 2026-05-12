namespace Atlas.Identity.Application.Tenants.UseCases.TenantIntegrationEvents;

public interface ITenantIntegrationEventsDispatcher
{
    Task ExecuteAsync(CancellationToken ct);
}
