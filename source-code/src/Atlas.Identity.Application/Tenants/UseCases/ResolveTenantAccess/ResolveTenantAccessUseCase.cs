using Atlas.Identity.Application.Tenants.Errors;
using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Domain.ValueObjects;
using Atlas.SharedKernel.Application.Events;
using Atlas.SharedKernel.Application.UseCases;

namespace Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;

public sealed class ResolveTenantAccessUseCase : IResolveTenantAccessUseCase
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IResultService _result;
    private readonly IDomainEventCollector _domainEventCollector;

    public ResolveTenantAccessUseCase(ITenantRepository tenantRepository, IResultService result, IDomainEventCollector domainEventCollector)
    {
        _tenantRepository = tenantRepository;
        _result = result;
        _domainEventCollector = domainEventCollector;
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

        _domainEventCollector.Collect(tenant.DomainEvents);

        return _result.Success(output);
    }
}
