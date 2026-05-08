using Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Workflow.Tenants;

public interface IResolveAccessWorkflow
{
    Task<ResolveAccessResultDto> ExecuteAsync(
        ResolveAccessCommand command,
        CancellationToken ct);
}
