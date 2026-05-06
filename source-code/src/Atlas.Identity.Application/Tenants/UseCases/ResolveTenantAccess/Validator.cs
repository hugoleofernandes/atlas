using FluentValidation;

namespace Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;

public sealed class Validator
    : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.TenantName)
            .NotEmpty();

        RuleFor(x => x.ExternalOid)
            .NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
