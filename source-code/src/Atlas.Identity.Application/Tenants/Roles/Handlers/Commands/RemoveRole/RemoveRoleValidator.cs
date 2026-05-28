using FluentValidation;

namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Commands.RemoveRole;

public sealed class RemoveRoleValidator : AbstractValidator<RemoveRoleCommand>
{
    public RemoveRoleValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
    }
}
