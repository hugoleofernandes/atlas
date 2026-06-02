using FluentValidation;

namespace Atlas.Identity.Application.Commands.DeactivateRole;

public sealed class DeactivateRoleValidator : AbstractValidator<DeactivateRoleCommand>
{
    public DeactivateRoleValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
    }
}
