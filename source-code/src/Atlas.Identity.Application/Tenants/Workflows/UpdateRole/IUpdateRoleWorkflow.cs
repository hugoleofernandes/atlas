using Atlas.Identity.Application.Tenants.Commands.UpdateRole;
using Atlas.SharedKernel.Application.Commands;

namespace Atlas.Identity.Application.Tenants.Workflows.UpdateRole;

public interface IUpdateRoleWorkflow
{
    Task<Result<UpdateRoleOutput>> ExecuteAsync(UpdateRoleCommand cmd, CancellationToken ct);
}
