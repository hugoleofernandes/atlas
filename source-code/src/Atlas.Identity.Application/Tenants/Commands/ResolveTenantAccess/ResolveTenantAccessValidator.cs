using FluentValidation;

namespace Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;

public sealed class ResolveTenantAccessValidator
    : AbstractValidator<ResolveTenantAccessCommand>
{
    public ResolveTenantAccessValidator()
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
