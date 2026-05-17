using Atlas.BuildingBlocks.Infrastructure.Validation;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;
using Atlas.SharedKernel.Application.Commands;
using FluentValidation;

namespace Atlas.Identity.Application.Tenants.Workflows.ResolveTenantAccess;

public sealed class ResolveTenantAccessWorkflow : IResolveTenantAccessWorkflow
{
    private readonly IValidator<Command> _validator;
    private readonly ICommandHandler _commandHandler;
    private readonly IIdentityUnitOfWork _uow;

    public ResolveTenantAccessWorkflow(
        IValidator<Command> validator,
        ICommandHandler commandHandler,
        IIdentityUnitOfWork uow)
    {
        _validator = validator;
        _commandHandler = commandHandler;
        _uow = uow;
    }

    public async Task<Result<Output>> ExecuteAsync(Command cmd, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return Result.Fail<Output>(validation.ToErrorDefinition());

        var output = await _commandHandler.ExecuteAsync(cmd, ct);

        await _uow.SaveChangesAsync(ct);

        return Result.Ok(output);
    }
}
