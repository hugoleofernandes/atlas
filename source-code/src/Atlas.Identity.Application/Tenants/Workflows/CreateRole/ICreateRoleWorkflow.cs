using Atlas.Identity.Application.Tenants.Commands.CreateRole;
using Atlas.SharedKernel.Application.Commands;

namespace Atlas.Identity.Application.Tenants.Workflows.CreateRole;

public interface ICreateRoleWorkflow
{
    Task<Result<Output>> ExecuteAsync(Command cmd, CancellationToken ct);
}
