using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.IntegrationEvents;
using FluentValidation;

namespace Atlas.Identity.Application.Tenants.Workflows.ResolveTenantAccess;

public sealed class ResolveTenantAccessWorkflow : IResolveTenantAccessWorkflow
{
    private readonly IValidator<Command> _validator;
    private readonly ICommandHandler _commandHandler;
    private readonly IIdentityUnitOfWork _uow;
    private readonly IResultService _result;
    private readonly IIntegrationEventEnqueuer _integrationEventEnqueuer;

    public ResolveTenantAccessWorkflow(
        IValidator<Command> validator,
        ICommandHandler commandHandler,
        IIdentityUnitOfWork uow,
        IResultService result,
        IIntegrationEventEnqueuer integrationEventEnqueuer)
    {
        _validator = validator;
        _commandHandler = commandHandler;
        _uow = uow;
        _result = result;
        _integrationEventEnqueuer = integrationEventEnqueuer;
    }

    public async Task<Result<Output>> ExecuteAsync(Command cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(cmd, ct);

        var result = await _commandHandler.ExecuteAsync(cmd, ct);

        if (!result.IsSuccess)
            return result;

        var domainEvents = _uow.GetDomainEvents();

        await _integrationEventEnqueuer.EnqueueAsync(domainEvents, ct);

        await _uow.SaveChangesAsync(ct);

        return _result.Success(result.Value!);
    }
}
