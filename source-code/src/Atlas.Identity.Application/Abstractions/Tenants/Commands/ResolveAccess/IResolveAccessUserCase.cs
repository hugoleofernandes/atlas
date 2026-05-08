using Atlas.Identity.Application.Tenants.Commands.ResolveAccess.UserCase;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Abstractions.Tenants.Commands.ResolveAccess;

public interface IResolveAccessUserCase
{
    Task<Response<Output>> ExecuteAsync(
        Command command,
        CancellationToken ct);
}
