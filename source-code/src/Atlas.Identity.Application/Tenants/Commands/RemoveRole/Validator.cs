using FluentValidation;

namespace Atlas.Identity.Application.Tenants.Commands.RemoveRole;

public sealed class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
    }
}
