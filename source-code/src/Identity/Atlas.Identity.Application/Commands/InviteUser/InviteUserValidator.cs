using FluentValidation;

namespace Atlas.Identity.Application.Commands.InviteUser;

public sealed class InviteUserValidator : AbstractValidator<InviteUserCommand>
{
    public InviteUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.RoleId)
            .NotEmpty();
    }
}
