# Result<T> & Errors

## Rules

✅ All handlers return `Result<TOutput>` — the invoker guarantees this contract
✅ Domain errors are raised by throwing a `DomainException` subclass — never `return Result.Fail` from domain code
✅ `DomainException` subclasses live in `Atlas.{Module}.Domain/{Aggregate}/Exceptions/`
✅ Every `DomainException` has a `const string ErrorCode` that matches a key in a `.resx` file
✅ Use `Result.Map` to transform the success value without unwrapping
❌ Never throw exceptions across handler boundaries — `DomainExceptionDecorator` catches and converts them
❌ Never duplicate the `ErrorCategory → HTTP status` mapping — use `ErrorCategoryExtensions.ToHttpStatus()`
❌ Never check `result.Value` without checking `result.IsSuccess` first

## ErrorCategory → HTTP Status

| Category | HTTP | When |
|---|---|---|
| `Validation` | 400 | Malformed input, format violations |
| `NotFound` | 404 | Resource does not exist |
| `Conflict` | 409 | Duplicate, already used |
| `Business` | 422 | Valid input but operation not allowed |
| `Unauthorized` | 401 | Auth failure |
| `Unexpected` | 500 | Unhandled / infrastructure error |

## DomainException Pattern

```csharp
// Atlas.Identity.Domain/Invitations/Exceptions/DuplicateInvitationException.cs
public sealed class DuplicateInvitationException : DomainException
{
    public const string ErrorCode = "invitation.duplicate";  // ← must match resx key

    public DuplicateInvitationException(string email)
        : base(ErrorCode, ErrorCategory.Conflict, $"Duplicate invitation for '{email}'.") { }
}
```

Add the key to `{Aggregate}Errors.resx` (EN) and `{Aggregate}Errors.pt.resx` (PT).

## Handler Return Pattern

```csharp
// ✅ success
return Result.Ok(new InviteUserOutput(...));

// ✅ explicit failure (when not using DomainException)
return Result.Fail<InviteUserOutput>(someErrorDefinition);

// ✅ transform without unwrapping
var result = await _invoker.InvokeAsync(_handler, query, ct);
return result.Map(output => MyResponse.From(output));
```

## Flow

```
Handler throws DuplicateInvitationException
  └─ DomainExceptionDecorator → Result.Fail(ErrorCategory.Conflict)
       └─ AtlasEndpoint reads IsSuccess == false
            └─ ToHttpStatus(Conflict) → 409 Problem Details (localized)
```
