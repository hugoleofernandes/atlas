namespace Atlas.Identity.Application.Tenants.Commands.RemoveRole;

public interface ICommandHandler
{
    Task<Output> ExecuteAsync(Command cmd, CancellationToken ct);
}
