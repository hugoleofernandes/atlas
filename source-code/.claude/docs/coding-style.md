# Coding Style

## Rules

✅ Keep workflows linear and top-to-bottom — every step visible inline
✅ Use narrative method names that describe exactly what happens
✅ Extract a method only when logic is complex, the name adds real clarity, or it is reused in 2+ places
❌ Never create helper methods that wrap a single obvious call (`AppendAuditLogsAsync`, `AddOutboxMessageAsync`)
❌ Never use generic verbs for method names: `Handle`, `Process`, `Manage`, `Do`, `Execute`
❌ Never split a readable inline expression into a private method just to "organize"

## Naming Pattern

```csharp
// ✅ narrative — describes exactly what happens
private async Task AddOutboxMessageForUserCreatedFromInvitationAsync(...) { }
private async Task SeedRolesAsync(CancellationToken ct) { }

// ❌ generic — applies to anything, conveys nothing
private async Task HandleAsync(...) { }
private async Task ProcessEventAsync(...) { }
private async Task ExecuteStepAsync(...) { }
```

## Workflow Pattern

A CommandHandler workflow reads top-to-bottom like a narrative — no hidden steps:

```csharp
// ✅ linear — all steps visible in sequence
var user = await _userRepository.GetByIdAsync(cmd.UserId, ct);
user.ChangeEmail(cmd.NewEmail);
await _uow.SaveChangesAsync(ct);
return Result.Ok(UserDto.From(user));

// ❌ steps hidden behind single-call wrappers
await ValidateAsync(cmd, ct);      // just wraps _validator.ValidateAsync
await PersistAsync(ct);            // just wraps _uow.SaveChangesAsync
```

## Method Extraction Rule

Extract only when **at least one** is true:
- The logic has multiple non-obvious steps
- The name conveys information the inline code does not
- The code is reused in two or more places

Otherwise: keep it inline.

## When Reviewing Code or Documents

❌ Do not list what is correct — assume the user already knows
✅ Go straight to problems, violations, and concrete improvements
✅ For every issue: problem → why it violates → corrected version → file/location
