using Atlas.Identity.Application.Tenants.Commands.CreateRole;
using Atlas.SharedKernel.Application.Commands;

namespace Atlas.Identity.Application.Tenants.Workflows.CreateRole;

public interface ICreateRoleWorkflow
{
    Task<Result<CreateRoleOutput>> ExecuteAsync(CreateRoleCommand cmd, CancellationToken ct);
}
