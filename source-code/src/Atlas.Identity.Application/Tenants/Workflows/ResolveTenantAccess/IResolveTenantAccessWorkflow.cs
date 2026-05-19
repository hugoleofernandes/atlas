using Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;
using Atlas.SharedKernel.Application.Commands;

namespace Atlas.Identity.Application.Tenants.Workflows.ResolveTenantAccess;

public interface IResolveTenantAccessWorkflow
{
    Task<Result<ResolveTenantAccessOutput>> ExecuteAsync(ResolveTenantAccessCommand cmd, CancellationToken ct);
}
