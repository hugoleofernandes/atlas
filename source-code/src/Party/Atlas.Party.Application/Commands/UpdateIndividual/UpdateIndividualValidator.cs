using FluentValidation;

namespace Atlas.Party.Application.Commands.UpdateIndividual;

public sealed class UpdateIndividualValidator : AbstractValidator<UpdateIndividualCommand>
{
    public UpdateIndividualValidator()
    {
        RuleFor(x => x.PartyId).NotEmpty();

        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.MiddleName).MaximumLength(100);
    }
}
