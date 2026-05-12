using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;
using Atlas.Identity.Application.Tenants.UseCases.TenantIntegrationEvents;
using Atlas.Identity.Domain.Entities.Audits;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.UseCases;
using FluentValidation;

namespace Atlas.Identity.Application.Tenants.Workflows.ResolveTenantAccess;

public sealed class ResolveTenantAccessWorkflow : IResolveTenantAccessWorkflow
{
    private readonly IValidator<Command> _validator;
    private readonly IResolveTenantAccessUseCase _useCase;
    private readonly IAuditService _auditService;
    private readonly IIdentityUnitOfWork _uow;
    private readonly IResultService _result;
    private readonly ITenantIntegrationEventsDispatcher _tenantEventDispatcher;


    public ResolveTenantAccessWorkflow(
        IValidator<Command> validator,
        IResolveTenantAccessUseCase useCase,
        IAuditService auditService,
        IIdentityUnitOfWork uow,
        IResultService result,
        ITenantIntegrationEventsDispatcher tenantEventDispatcher)
    {
        _validator = validator;
        _useCase = useCase;
        _auditService = auditService;
        _uow = uow;
        _result = result;
        _tenantEventDispatcher = tenantEventDispatcher;
    }

    public async Task<Result<Output>> ExecuteAsync(Command cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(cmd, ct);

        var useCaseResult = await _useCase.ExecuteAsync(cmd, ct);

        if (!useCaseResult.IsSuccess)
            return useCaseResult;

        await _tenantEventDispatcher.ExecuteAsync(ct);

        await _auditService.AddAuditLogsAsync<IdentityModuleAudit>(_uow, ct);

        await _uow.SaveChangesAsync(ct);

        return _result.Success(useCaseResult.Value!);
    }
}