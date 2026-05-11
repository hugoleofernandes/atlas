using Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;
using Atlas.SharedKernel.Application.UseCases;

namespace Atlas.Identity.Application.Tenants.Workflows.ResolveTenantAccess;

public interface IResolveTenantAccessWorkflow
{
    Task<Result<Output>> ExecuteAsync(Command cmd, CancellationToken ct);
}
