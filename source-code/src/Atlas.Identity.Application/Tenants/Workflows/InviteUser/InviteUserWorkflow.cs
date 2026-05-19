using Atlas.BuildingBlocks.Infrastructure.Workflows;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Commands.InviteUser;
using Atlas.SharedKernel.Application.Commands;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Application.Tenants.Workflows.InviteUser;

public sealed class InviteUserWorkflow : WorkflowBase<InviteUserCommand, InviteUserOutput>, IInviteUserWorkflow
{
    private readonly IInviteUserCommandHandler _commandHandler;
    private readonly IIdentityUnitOfWork _uow;

    public InviteUserWorkflow(
        IValidator<InviteUserCommand> validator,
        IInviteUserCommandHandler commandHandler,
        IIdentityUnitOfWork uow,
        ILoggerFactory loggerFactory) : base(validator, loggerFactory)
    {
        _commandHandler = commandHandler;
        _uow = uow;
    }

    protected override async Task<InviteUserOutput> HandleAsync(InviteUserCommand cmd, CancellationToken ct)
    {
        var output = await _commandHandler.ExecuteAsync(cmd, ct);
        await _uow.SaveChangesAsync(ct);
        return output;
    }
}
