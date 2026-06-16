using FluentValidation;

namespace Atlas.Party.Application.Commands.RegisterIndividual;

public sealed class RegisterIndividualValidator : AbstractValidator<RegisterIndividualCommand>
{
    public RegisterIndividualValidator()
    {
        RuleFor(x => x.TaxNumber).NotEmpty();

        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.MiddleName).MaximumLength(100);
    }
}
