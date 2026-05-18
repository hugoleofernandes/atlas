using Atlas.BuildingBlocks.Infrastructure.Workflows;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Commands.CreateRole;
using Atlas.SharedKernel.Application.Commands;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Application.Tenants.Workflows.CreateRole;

public sealed class CreateRoleWorkflow : WorkflowBase<Command, Output>, ICreateRoleWorkflow
{
    private readonly ICommandHandler _commandHandler;
    private readonly IIdentityUnitOfWork _uow;

    public CreateRoleWorkflow(
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
