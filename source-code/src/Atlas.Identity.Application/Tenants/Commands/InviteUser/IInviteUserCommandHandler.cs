namespace Atlas.Identity.Application.Tenants.Commands.InviteUser;

public interface IInviteUserCommandHandler
{
    Task<InviteUserOutput> ExecuteAsync(InviteUserCommand cmd, CancellationToken ct);
}
