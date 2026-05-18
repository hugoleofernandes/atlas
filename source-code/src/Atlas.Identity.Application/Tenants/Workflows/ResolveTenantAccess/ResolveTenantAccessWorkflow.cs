using Atlas.BuildingBlocks.Infrastructure.Workflows;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;
using Atlas.SharedKernel.Application.Commands;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Application.Tenants.Workflows.ResolveTenantAccess;

public sealed class ResolveTenantAccessWorkflow : WorkflowBase<Command, Output>, IResolveTenantAccessWorkflow
{
    private readonly ICommandHandler _commandHandler;
    private readonly IIdentityUnitOfWork _uow;

    public ResolveTenantAccessWorkflow(
        IValidator<Command> validator,
        ICommandHandler commandHandler,
        IIdentityUnitOfWork uow,
        ILoggerFactory loggerFactory) : base(validator, loggerFactory)
    {
        _commandHandler = commandHandler;
        _uow = uow;
    }

    protected override async Task<Output> HandleAsync(Command cmd, CancellationToken ct)
    {
        var output = await _commandHandler.ExecuteAsync(cmd, ct);
        await _uow.SaveChangesAsync(ct);
        return output;
    }
}
