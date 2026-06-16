using FluentValidation;

namespace Atlas.Party.Application.Commands.RegisterOrganization;

public sealed class RegisterOrganizationValidator : AbstractValidator<RegisterOrganizationCommand>
{
    public RegisterOrganizationValidator()
    {
        RuleFor(x => x.TaxNumber).NotEmpty();

        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.TradeName).MaximumLength(200);
    }
}
