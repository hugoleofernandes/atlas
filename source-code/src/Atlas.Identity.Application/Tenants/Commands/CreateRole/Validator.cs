using FluentValidation;

namespace Atlas.Identity.Application.Tenants.Commands.CreateRole;

public sealed class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(10);

        RuleFor(x => x.PermissionCodes)
            .NotNull();
    }
}
