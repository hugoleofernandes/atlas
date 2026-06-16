using FluentValidation;

namespace Atlas.Party.Application.Commands.DeactivateIndividual;

public sealed class DeactivateIndividualValidator : AbstractValidator<DeactivateIndividualCommand>
{
    public DeactivateIndividualValidator()
    {
        RuleFor(x => x.PartyId).NotEmpty();
    }
}
