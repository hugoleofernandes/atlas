using FluentValidation;

namespace Atlas.Identity.Application.Tenants.Commands.RemoveRole;

public sealed class RemoveRoleValidator : AbstractValidator<RemoveRoleCommand>
{
    public RemoveRoleValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
    }
}
