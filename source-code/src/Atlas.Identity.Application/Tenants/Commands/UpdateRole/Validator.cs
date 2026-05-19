using FluentValidation;

namespace Atlas.Identity.Application.Tenants.Commands.UpdateRole;

public sealed class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(10);

        RuleFor(x => x.PermissionCodes)
            .NotNull();
    }
}
