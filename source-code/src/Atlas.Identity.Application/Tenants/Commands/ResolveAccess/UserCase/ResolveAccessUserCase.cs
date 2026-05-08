using Atlas.Identity.Application.Abstractions.Repositories;
using Atlas.Identity.Application.Abstractions.Tenants.Commands.ResolveAccess;
using Atlas.Identity.Application.Errors;
using Atlas.Identity.Application.Tenants.Commands.ResolveAccess.UserCase;
using Atlas.Identity.Domain.ValueObjects;
using Atlas.SharedKernel.Application;

public sealed class ResolveAccessUserCase : IResolveAccessUserCase
{
    private readonly ITenantRepository _tenantRepository;

    public ResolveAccessUserCase(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Response<Output>> ExecuteAsync(
        Command command,
        CancellationToken ct)
    {
        var tenant = await _tenantRepository
            .GetByNameWithUsersAndInvitationsAsync(
                command.TenantName.ToLowerInvariant(), ct);

        if (tenant is null)
            return Response<Output>.Failure(TenantErrors.NotFound);

        var user = tenant.ResolveAccess(
            ExternalId.Create(command.ExternalOid),
            Email.Create(command.Email));

        var result = new Result(
            tenant.Id,
            tenant.Name,
            user.Id,
            user.Role.Value);

        return Response<Output>.Ok(new Output(tenant, result));
    }
}
