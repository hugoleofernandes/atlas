# Validation

## Rules
✅ Validator validates **input shape only** — format, length, required fields
✅ Validator class lives in the same folder as its command handler
✅ Naming: `{CommandName}Validator : AbstractValidator<{CommandName}Command>`
✅ Registration is automatic via assembly scan — no manual DI wiring per validator
❌ Never validate business rules in a validator — uniqueness, ownership, existence go in the handler or domain
❌ Never create a validator just to check `NotEmpty` on a single `Guid` — that is better caught as domain `NotFound`
❌ Never create an empty validator class

## Pattern

```csharp
// Commands/InviteUser/InviteUserValidator.cs
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
```

## Anti-patterns

```csharp
// ❌ business rule in validator — requires DB call, belongs in handler
RuleFor(x => x.Email)
    .MustAsync(async (email, ct) => !await _userRepo.ExistsWithEmailAsync(email, ct))
    .WithMessage("Email already registered.");

// ❌ wrong location — validator is in a shared/common folder
// Validators/InviteUserValidator.cs  ← never here

// ❌ empty validator — adds nothing, remove it
public sealed class RemoveRoleValidator : AbstractValidator<RemoveRoleCommand>
{
    public RemoveRoleValidator() { }
}
```

## How It Runs

The `ValidationDecorator` in the `HandlerInvoker` pipeline resolves `IValidator<TCommand>` from DI before the handler executes.
No validator registered → no-op, handler runs normally.
Validation failure → `Result.Fail(ErrorCategory.Validation)` → HTTP 400. Handler never runs.
