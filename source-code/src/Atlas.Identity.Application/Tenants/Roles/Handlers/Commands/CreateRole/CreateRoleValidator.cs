using FluentValidation;

namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Commands.CreateRole;

public sealed class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(10);

        RuleFor(x => x.PermissionCodes)
            .NotNull();
    }
}
