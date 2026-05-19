using Atlas.BuildingBlocks.Infrastructure.Workflows;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Commands.RemoveRole;
using Atlas.SharedKernel.Application.Commands;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Application.Tenants.Workflows.RemoveRole;

public sealed class RemoveRoleWorkflow : WorkflowBase<RemoveRoleCommand, RemoveRoleOutput>, IRemoveRoleWorkflow
{
    private readonly IRemoveRoleCommandHandler _commandHandler;
    private readonly IIdentityUnitOfWork _uow;

    public RemoveRoleWorkflow(
        IValidator<RemoveRoleCommand> validator,
        IRemoveRoleCommandHandler commandHandler,
        IIdentityUnitOfWork uow,
        ILoggerFactory loggerFactory) : base(validator, loggerFactory)
    {
        _commandHandler = commandHandler;
        _uow = uow;
    }

    protected override async Task<RemoveRoleOutput> HandleAsync(RemoveRoleCommand cmd, CancellationToken ct)
    {
        var output = await _commandHandler.ExecuteAsync(cmd, ct);
        await _uow.SaveChangesAsync(ct);
        return output;
    }
}
