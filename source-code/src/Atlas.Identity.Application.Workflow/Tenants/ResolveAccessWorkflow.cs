using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Tenants.Abstractions;
using Atlas.Identity.Application.Tenants.Abstractions.ResolveAccess;
using Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;
using Atlas.Identity.Domain.Entities.Tenants.Events;
using FluentValidation;

public sealed class ResolveAccessWorkflow : IResolveAccessWorkflow
{
    private readonly IResolveAccessUseCase _useCase;
    private readonly IValidator<Command> _validator;
    private readonly IIdentityUnitOfWork _uow;

    public ResolveAccessWorkflow(
        IResolveAccessUseCase useCase,
        IValidator<Command> validator,
        IIdentityUnitOfWork uow)
    {
        _useCase = useCase;
        _validator = validator;
        _uow = uow;
    }

    public async Task<ResultDto> ExecuteAsync(Command cmd, CancellationToken ct)
    {
        // 1. Validação explícita
        _validator.ValidateAndThrow(cmd);

        // 2. Executa o caso de uso (sem SaveChanges)
        var result = await _useCase.ExecuteAsync(cmd, ct);

        // 3. Captura eventos do Tenant
        var events = result.Tenant.DomainEvents.ToList();

        // 4. Orquestração explícita
        foreach (var evt in events)
        {
            if (evt is UserCreatedFromInvitationDomainEvent e)
            {
                // chamar outro use case, enviar email, auditar, etc.
            }
        }

        // 5. Persistência única (transação + outbox)
        await _uow.SaveChangesAsync(ct);

        return result.Dto;
    }
}
