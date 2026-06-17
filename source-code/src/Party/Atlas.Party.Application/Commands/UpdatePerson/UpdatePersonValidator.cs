using FluentValidation;

namespace Atlas.Party.Application.Commands.UpdatePerson;

public sealed class UpdatePersonValidator : AbstractValidator<UpdatePersonCommand>
{
    public UpdatePersonValidator()
    {
        RuleFor(x => x.PartyId).NotEmpty();

        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.MiddleName).MaximumLength(100);
    }
}

