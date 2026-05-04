# DDD Commenting Guidelines

## Goal

Explain:
- Why the class exists
- Business rules (invariants)
- Boundaries

Avoid:
- Explaining obvious code
- Redundant comments

---

## Aggregate Root

Must include:
- Purpose
- Invariants
- Design decisions

---

## Value Object

Must include:
- Immutability
- Validation rules

---

## Use Cases

Must include:
- Responsibilities
- What it does NOT do

---

## Example

/// <summary>
/// Represents a tenant.
///
/// Invariants:
/// - Must always have at least one owner
/// </summary>