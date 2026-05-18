using Atlas.Identity.Application.Tenants.Commands.InviteUser;
using Atlas.SharedKernel.Application.Commands;

namespace Atlas.Identity.Application.Tenants.Workflows.InviteUser;

public interface IInviteUserWorkflow
{
    Task<Result<Output>> ExecuteAsync(Command cmd, CancellationToken ct);
}
