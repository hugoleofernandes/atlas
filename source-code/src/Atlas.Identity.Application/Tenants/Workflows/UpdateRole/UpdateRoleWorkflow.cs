using Atlas.BuildingBlocks.Infrastructure.Workflows;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Commands.UpdateRole;
using Atlas.SharedKernel.Application.Commands;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Application.Tenants.Workflows.UpdateRole;

public sealed class UpdateRoleWorkflow : WorkflowBase<UpdateRoleCommand, UpdateRoleOutput>, IUpdateRoleWorkflow
{
    private readonly IUpdateRoleCommandHandler _commandHandler;
    private readonly IIdentityUnitOfWork _uow;

    public UpdateRoleWorkflow(
        IValidator<UpdateRoleCommand> validator,
        IUpdateRoleCommandHandler commandHandler,
        IIdentityUnitOfWork uow,
        ILoggerFactory loggerFactory) : base(validator, loggerFactory)
    {
        _commandHandler = commandHandler;
        _uow = uow;
    }

    protected override async Task<UpdateRoleOutput> HandleAsync(UpdateRoleCommand cmd, CancellationToken ct)
    {
        var output = await _commandHandler.ExecuteAsync(cmd, ct);
        await _uow.SaveChangesAsync(ct);
        return output;
    }
}
