using Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;
using Atlas.SharedKernel.Application.Commands;

namespace Atlas.Identity.Application.Tenants.Workflows.ResolveTenantAccess;

public interface IResolveTenantAccessWorkflow
{
    Task<Result<Output>> ExecuteAsync(Command cmd, CancellationToken ct);
}
