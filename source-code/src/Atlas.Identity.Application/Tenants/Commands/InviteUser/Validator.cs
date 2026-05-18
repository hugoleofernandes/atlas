using FluentValidation;

namespace Atlas.Identity.Application.Tenants.Commands.InviteUser;

public sealed class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.RoleId)
            .NotEmpty();
    }
}
