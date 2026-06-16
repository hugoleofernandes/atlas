using FluentValidation;

namespace Atlas.Party.Application.Commands.DeactivateOrganization;

public sealed class DeactivateOrganizationValidator : AbstractValidator<DeactivateOrganizationCommand>
{
    public DeactivateOrganizationValidator()
    {
        RuleFor(x => x.PartyId).NotEmpty();
    }
}
