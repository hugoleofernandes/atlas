namespace Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;

public interface IResolveTenantAccessUseCase
{
    Task<ResolveTenantAccessResult> ExecuteAsync(
        ResolveTenantAccessCommand command,
        CancellationToken ct);
}