using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Application.Tenants.Services.Errors;
using Atlas.Identity.Domain.ValueObjects;
using Atlas.SharedKernel.Application.Commands;

namespace Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;

public sealed class CommandHandler : ICommandHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IResultService _result;

    public CommandHandler(ITenantRepository tenantRepository, IResultService result)
    {
        _tenantRepository = tenantRepository;
        _result = result;
    }

    public async Task<Result<Output>> ExecuteAsync(Command cmd, CancellationToken ct)
    {
        var tenant = await _tenantRepository
            .GetByNameWithUsersAndInvitationsAsync(
                cmd.TenantName.ToLowerInvariant(), ct);

        if (tenant is null)
            return _result.Failure<Output>(TenantErrors.NotFound);

        var user = tenant.ResolveAccess(
            ExternalId.Create(cmd.ExternalOid),
            Email.Create(cmd.Email));

        var output = new Output(
            tenant.Id,
            tenant.Name,
            user.Id,
            user.Role.Value);

        return _result.Success(output);
    }
}
