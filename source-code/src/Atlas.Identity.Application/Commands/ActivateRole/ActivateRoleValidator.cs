using FluentValidation;

namespace Atlas.Identity.Application.Commands.ActivateRole;

public sealed class ActivateRoleValidator : AbstractValidator<ActivateRoleCommand>
{
    public ActivateRoleValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
    }
}
