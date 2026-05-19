using Atlas.Identity.Application.Tenants.Commands.InviteUser;
using Atlas.SharedKernel.Application.Commands;

namespace Atlas.Identity.Application.Tenants.Workflows.InviteUser;

public interface IInviteUserWorkflow
{
    Task<Result<InviteUserOutput>> ExecuteAsync(InviteUserCommand cmd, CancellationToken ct);
}
