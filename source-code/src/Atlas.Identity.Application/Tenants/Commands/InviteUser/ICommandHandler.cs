namespace Atlas.Identity.Application.Tenants.Commands.InviteUser;

public interface ICommandHandler
{
    Task<Output> ExecuteAsync(Command cmd, CancellationToken ct);
}
