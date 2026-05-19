using Atlas.Identity.Application.Tenants.Commands.RemoveRole;
using Atlas.SharedKernel.Application.Commands;

namespace Atlas.Identity.Application.Tenants.Workflows.RemoveRole;

public interface IRemoveRoleWorkflow
{
    Task<Result<Output>> ExecuteAsync(Command cmd, CancellationToken ct);
}
