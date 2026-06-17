using FluentValidation;

namespace Atlas.Party.Application.Commands.DeactivatePerson;

public sealed class DeactivatePersonValidator : AbstractValidator<DeactivatePersonCommand>
{
    public DeactivatePersonValidator()
    {
        RuleFor(x => x.PartyId).NotEmpty();
    }
}

