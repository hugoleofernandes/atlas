using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Abstractions.Tenants.Commands.ResolveAccess;
using Atlas.Identity.Application.Errors;
using Atlas.Identity.Application.Tenants.Commands.ResolveAccess.UserCase;
using Atlas.Identity.Domain.Entities.Audits;
using Atlas.Identity.Domain.Entities.Tenants.Events;
using Atlas.SharedKernel.Application;
using FluentValidation;

public sealed class ResolveAccessWorkflow : IResolveAccessWorkflow
{
    private readonly IValidator<Command> _validator;
    private readonly IResolveAccessUserCase _useCase;
    private readonly IAuditService _auditService;
    private readonly IIdentityUnitOfWork _uow;

    public ResolveAccessWorkflow(
        IValidator<Command> validator,
        IResolveAccessUserCase useCase,
        IAuditService auditService,
        IIdentityUnitOfWork uow)
    {
        _validator = validator;
        _useCase = useCase;
        _auditService = auditService;
        _uow = uow;
    }

    public async Task<Response<Result>> ExecuteAsync(Command cmd, CancellationToken ct)
    {
        _validator.ValidateAndThrow(cmd);

        var usercaseResult = await _useCase.ExecuteAsync(cmd, ct);

        if (!usercaseResult.IsSuccess)
            return Response<Result>.Failure(TenantErrors.ResolveAccess);

        var output = usercaseResult.Value!;

        if (output.GetEvent<UserCreatedFromInvitationDomainEvent>() is { } evt)
            await _uow.AddOutboxMessage(evt.ToOutboxMessage());

        await _auditService.AddAuditLogsAsync<IdentityModuleAudit>(_uow, ct);

        await _uow.SaveChangesAsync(ct);

        return Response<Result>.Ok(output.Result);
    }
}
