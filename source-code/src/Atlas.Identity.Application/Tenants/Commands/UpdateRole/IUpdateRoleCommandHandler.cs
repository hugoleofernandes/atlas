namespace Atlas.Identity.Application.Tenants.Commands.UpdateRole;

public interface IUpdateRoleCommandHandler
{
    Task<UpdateRoleOutput> ExecuteAsync(UpdateRoleCommand cmd, CancellationToken ct);
}
