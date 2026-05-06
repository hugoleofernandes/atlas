# DDD Unit Testing Guidelines

## Language Rule
All documentation and test names must be written in English.

---

## Purpose
This guideline defines how unit tests must be written for Domain‑Driven Design (DDD) systems.  
Its goal is to ensure that tests validate **domain behavior**, **invariants**, and **business rules**, not implementation details.

---

## Principles
1. Test behavior, not methods  
2. Test invariants, not branches  
3. Test domain rules, not technical details  
4. Test state transitions, not internal fields  
5. Test events emitted, not how they are emitted  
6. Test exceptions as business rules, not as errors  
7. One test = one business rule  
8. Tests must be deterministic and unambiguous  

---

## What to Test (Domain Layer)

### 1. Invariants
Every invariant in the aggregate must have at least one test.

Examples:
- A tenant cannot have two active invitations for the same email  
- A tenant cannot invite an existing user  
- A tenant cannot operate when inactive  

---

### 2. Domain Behavior
Every domain method must have tests covering:

- Happy path  
- Alternative valid paths  
- Invalid paths (exceptions)  
- Event emission  

---

### 3. Domain Events
Tests must verify:

- Which events are emitted  
- In what order (if relevant)  
- With what data  

---

### 4. State Transitions
Tests must verify:

- What changed  
- What did not change  
- What must always remain true  

---

### 5. Exceptions as Business Rules
Exceptions represent invariant violations.

Tests must verify:
- The correct exception type  
- The correct business rule message (if applicable)  

---

## What to Test (Application Layer)

Application tests must validate orchestration only:

- The correct repository method is called  
- The correct aggregate method is invoked  
- The Unit of Work is committed  
- The correct DTO is returned  
- Domain exceptions are propagated  
- No business logic is implemented in the application layer  

---

## What NOT to Test
- Private methods  
- Internal data structures  
- EF Core behavior  
- Repository behavior  
- Infrastructure concerns  
- Logging  
- Serialization  
- DTOs  
- Controllers  
- Business rules in the application layer  
- Invariants in the application layer  
- Domain behavior in the application layer  
- Domain events in the application layer  

---

## Boundaries of Unit Testing

### This guideline covers:
- How to test domain behavior  
- How to test invariants  
- How to test domain events  
- How to test application orchestration  
- Naming, structure, and consistency rules  

### This guideline does NOT cover:
- Integration testing  
- Application testing in depth (see Application Testing Guidelines)  
- Infrastructure testing  
- End‑to‑end testing  
- Performance testing  

---

## Test Naming

Use the pattern:

MethodName_ShouldExpectedBehavior_WhenCondition


Examples:
- `InviteUser_ShouldThrow_WhenUserAlreadyExists`  
- `ResolveAccess_ShouldCreateUser_WhenInvitationExists`  
- `ResolveAccess_ShouldReturnExistingUser_WhenUserAlreadyExists`  
- `InviteUser_ShouldThrow_WhenActiveInvitationExists`  

---

## Test Structure (AAA)

### Arrange
- Create aggregate  
- Set initial state  
- Add invitations/users if needed  

### Act
- Call the domain method  

### Assert
- Validate invariants  
- Validate events  
- Validate state transitions  
- Validate exceptions  

---

## Example Test

<codeblock language="csharp">
[Fact]
public void InviteUser_ShouldThrow_WhenActiveInvitationAlreadyExists()
{
    var tenant = new Tenant("test");

    tenant.InviteUser("user@test.com", "admin", TimeSpan.FromHours(1));

    var act = () => tenant.InviteUser("user@test.com", "admin", TimeSpan.FromHours(1));

    act.Should().Throw<DuplicateInvitationException>();
}
</codeblock>

---

## Checklist

Before finishing a domain feature, verify:

- [ ] All invariants have tests  
- [ ] All domain behaviors have tests  
- [ ] All exceptions have tests  
- [ ] All events have tests  
- [ ] All state transitions have tests  
- [ ] All edge cases are covered  
- [ ] No infrastructure is tested  
- [ ] No implementation detail is tested  

---

## Anti‑Patterns
❌ Testing private methods  
❌ Testing EF Core behavior  
❌ Mocking domain entities  
❌ Asserting internal fields  
❌ Overusing mocks  
❌ Testing the same rule in multiple places  
❌ Writing tests that break when refactoring  
❌ Testing implementation instead of behavior  

---

## AI Usage Rules

When generating unit tests, AI must:

### Always:
- Test invariants first  
- Test exceptions as business rules  
- Test events emitted  
- Follow naming conventions  
- Follow AAA structure  
- Use the ubiquitous language  
- Keep tests deterministic and unambiguous  

### Never:
- Test implementation details  
- Generate redundant tests  
- Mock domain entities  
- Invent domain rules  
- Introduce new terminology  
- Test infrastructure or persistence  

---

## Final Notes
Unit tests must:
- Validate domain correctness  
- Strengthen the model  
- Protect invariants  
- Ensure behavior remains stable over time  

Unit tests are not for testing code — they are for testing **business rules**.
