using FluentValidation;

namespace Atlas.Party.Application.Commands.UpdateOrganization;

public sealed class UpdateOrganizationValidator : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationValidator()
    {
        RuleFor(x => x.PartyId).NotEmpty();

        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.TradeName).MaximumLength(200);
    }
}
