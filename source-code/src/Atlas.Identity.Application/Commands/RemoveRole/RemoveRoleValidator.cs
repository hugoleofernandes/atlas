using FluentValidation;

namespace Atlas.Identity.Application.Commands.RemoveRole;

public sealed class RemoveRoleValidator : AbstractValidator<RemoveRoleCommand>
{
    public RemoveRoleValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
    }
}
