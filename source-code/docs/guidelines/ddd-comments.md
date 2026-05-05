# DDD Commenting Guidelines

## Goal

Document **business intent**, not technical implementation.

Every comment must answer:
- Why does this exist?
- What rules does it enforce?
- What are its boundaries?

---

## Aggregate Root

Must include:

- Purpose (why it exists)
- Invariants (business rules that must always hold)
- Design decisions (non-obvious modeling choices)
- Boundaries (what it explicitly does NOT handle)

---

## Entity (inside aggregate)

Must include:

- Role within the aggregate
- Lifecycle ownership
- Key invariants (if any)

Avoid:
- Repeating rules already defined in the aggregate

---

## Value Object

Must include:

- Immutability guarantee
- Validation rules
- Domain meaning

---

## Use Cases (Application Layer)

Must include:

- Responsibilities
- Input/output expectations
- What it explicitly does NOT handle

---

## Method-Level Comments

Use XML comments only when:

- The method represents business behavior
- The intent is not obvious from the name
- There are important constraints or side effects

Avoid:
- Commenting trivial getters/setters
- Describing obvious operations

---

## Example

```csharp
/// <summary>
/// Authorizes an existing user within the tenant.
///
/// Invariants:
/// - User must be active
/// - User must be linked to the tenant
///
/// Throws:
/// - InvalidOperationException when user is not linked
/// </summary>