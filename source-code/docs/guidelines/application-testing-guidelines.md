# Application Testing Guidelines

## Language Rule
All documentation and test names must be written in English.

---

## Purpose
This guideline defines how application layer tests must be written.  
Its goal is to ensure that tests validate **orchestration**, **interactions**, and **collaboration**, not domain behavior or business rules.

---

## Principles
1. Test orchestration, not domain behavior  
2. Test interactions, not internal logic  
3. Test collaboration, not state  
4. Test propagation, not transformation  
5. Test DTO correctness, not domain correctness  
6. Test repository usage, not persistence  
7. Test Unit of Work commit, not transaction behavior  
8. Tests must be deterministic and unambiguous  

---

## What to Test

### 1. Repository Calls
Verify that the use case:
- Calls the correct repository method  
- Passes the correct parameters  
- Calls the repository exactly once (unless otherwise required)  

Examples:
- `GetByNameAsync`  
- `GetByIdAsync`  
- `AddAsync`  

---

### 2. Aggregate Method Invocation
Verify that the use case:
- Calls the correct domain method  
- Passes the correct arguments  
- Does NOT call domain methods that should not be used  

Example:
- `tenant.ResolveAccess(...)` must be invoked  

---

### 3. Unit of Work Commit
Verify that:
- `SaveChangesAsync()` is called  
- It is called exactly once  
- It is not called when the operation fails  

---

### 4. DTO Mapping
Verify that:
- The returned DTO contains the correct values  
- The DTO reflects the domain result  
- No domain objects leak into the application layer  

---

### 5. Exception Propagation
Verify that:
- Domain exceptions are propagated, not swallowed  
- Application layer does NOT convert domain exceptions into generic ones  
- Application layer does NOT catch domain exceptions unless explicitly required  

Examples:
- `InvitationNotFoundException`  
- `UserAlreadyExistsException`  

---

### 6. Input Normalization
Verify that:
- Emails are normalized  
- Names are normalized  
- External IDs are passed correctly  

---

## What NOT to Test
- Domain rules  
- Domain invariants  
- Domain events  
- Domain state transitions  
- Private methods  
- EF Core behavior  
- Repository implementation  
- Infrastructure concerns  
- Logging  
- Serialization  
- Controllers  
- HTTP behavior  
- Validation logic (unless part of the use case contract)  
- Input validation beyond the use case contract  

---

## Boundaries of Application Testing

### This guideline covers:
- How to test application orchestration  
- How to test repository interactions  
- How to test aggregate method invocation  
- How to test Unit of Work behavior  
- How to test DTO mapping and exception propagation  

### This guideline does NOT cover:
- Domain testing (see Unit Testing Guidelines)  
- Integration testing  
- Infrastructure testing  
- End‑to‑end testing  
- Domain behavior validation  

---

## Test Naming

Use the pattern:

UseCaseName_ShouldExpectedBehavior_WhenCondition


Examples:
- `ResolveTenantAccess_ShouldReturnUser_WhenInvitationExists`  
- `ResolveTenantAccess_ShouldThrow_WhenInvitationNotFound`  
- `InviteUserUseCase_ShouldCallRepository_WhenValidRequest`  
- `InviteUserUseCase_ShouldCommit_WhenOperationSucceeds`  

---

## Test Structure (AAA)

### Arrange
- Mock repositories  
- Mock Unit of Work  
- Mock domain aggregates (only to verify method invocation)  
- Prepare input DTO  

### Act
- Execute the use case  

### Assert
- Verify repository calls  
- Verify aggregate method calls  
- Verify Unit of Work commit  
- Verify DTO output  
- Verify exception propagation  

---

## Example Test

<codeblock language="csharp">
[Fact]
public async Task ResolveTenantAccess_ShouldCallResolveAccess_WhenTenantExists()
{
    // Arrange
    var tenant = Substitute.For<Tenant>("test");
    var repo = Substitute.For<ITenantRepository>();
    var uow = Substitute.For<IUnitOfWork>();

    repo.GetByNameWithUsersAndInvitationsAsync("test", Arg.Any<CancellationToken>())
        .Returns(tenant);

    var useCase = new ResolveTenantAccessUseCase(repo, uow);

    // Act
    await useCase.ExecuteAsync(new("test", "oid-123", "user@test.com"), CancellationToken.None);

    // Assert
    tenant.Received(1).ResolveAccess("oid-123", "user@test.com");
    await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
}
</codeblock>

---

## Checklist

Before finishing an application use case, verify:

- [ ] Repository methods are called correctly  
- [ ] Aggregate methods are invoked correctly  
- [ ] Unit of Work is committed  
- [ ] DTO output is correct  
- [ ] Domain exceptions are propagated  
- [ ] No domain logic is implemented  
- [ ] No infrastructure logic is tested  
- [ ] No internal state is asserted  
- [ ] No redundant mocks are used  

---

## Anti‑Patterns
❌ Testing domain rules in the application layer  
❌ Mocking domain entities to test domain behavior  
❌ Asserting internal state of aggregates  
❌ Testing EF Core behavior  
❌ Overusing mocks  
❌ Testing controllers instead of use cases  
❌ Swallowing domain exceptions  
❌ Returning domain objects directly  
❌ Mixing domain and application tests  

---

## Mocking Rules

### What to Mock
- **Repositories**  
  - To control which aggregates are returned  
  - To verify that the correct methods are called  

- **Unit of Work**  
  - To verify that `SaveChangesAsync()` is called (or not called)  

- **Domain Aggregates (carefully)**  
  - Only to verify that a specific domain method was invoked  
  - Never to simulate domain behavior or rules  

---

### What NOT to Mock
- Value Objects  
- Domain Entities (for behavior)  
- Domain logic  
- Domain events  
- Infrastructure implementations  

---

## When NOT to Write Application Tests

You may skip application tests when:

- The use case is a thin pass‑through with no orchestration  
  - Example: directly calling a single domain method and returning its result  

- The behavior is already fully covered by domain tests  
  - No additional orchestration, mapping, or coordination exists  

- The use case does not:  
  - Call multiple collaborators  
  - Coordinate repositories and unit of work  
  - Transform input/output in a meaningful way  

---

## Test Doubles Strategy

### Preferred Test Doubles
- **Mocks**  
  - For repositories and unit of work  
  - To verify interactions  

- **Stubs**  
  - For simple return values  

### Avoid
- Mocking domain entities to simulate behavior  
- Complex mock setups to reproduce domain rules  
- Mixing multiple types of test doubles without purpose  

### Principle
Use test doubles to verify **orchestration**, not to re‑implement domain logic.

---

## AI Usage Rules

When generating application tests, AI must:

### Always:
- Test orchestration  
- Mock repositories and Unit of Work  
- Propagate domain exceptions  
- Follow naming conventions  
- Follow AAA structure  
- Keep tests deterministic and unambiguous  

### Never:
- Test domain logic  
- Test implementation details  
- Generate redundant tests  
- Invent domain rules  
- Introduce new terminology  

---

## Final Notes
Application tests must:
- Validate orchestration  
- Ensure correct collaboration  
- Protect application boundaries  
- Guarantee correct propagation of domain behavior  

Application tests are not for testing domain rules — they are for testing **coordination**.
