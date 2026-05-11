using Atlas.SharedKernel.Application.UseCases;

namespace Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;

public interface IResolveTenantAccessUseCase
{
    Task<Result<Output>> ExecuteAsync(Command cmd, CancellationToken ct);
}
