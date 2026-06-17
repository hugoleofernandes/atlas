using FluentValidation;

namespace Atlas.Party.Application.Commands.RegisterPerson;

public sealed class RegisterPersonValidator : AbstractValidator<RegisterPersonCommand>
{
    public RegisterPersonValidator()
    {
        RuleFor(x => x.TaxNumber).NotEmpty();

        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.MiddleName).MaximumLength(100);
    }
}

