# Technical Decisions & Tooling

## Purpose
This document lists all external tools, libraries, and architectural decisions used in the project.  
It ensures clarity, onboarding efficiency, and long‑term maintainability.

---

## Tooling Overview
Summary of all tools used in the solution and the reason each one exists.

### Stryker.NET
**Purpose:** Mutation testing to validate the quality of domain tests.  
**Reason for adoption:** Ensures invariants and business rules are truly protected by tests.  
**Impact:** Improves reliability of domain tests; used during CI to enforce quality thresholds.

### DocFX
**Purpose:** Documentation generation from Markdown and XML comments.  
**Reason for adoption:** Native .NET support; aligns with project documentation guidelines.  
**Impact:** Produces a unified documentation site; improves onboarding.

### xUnit
**Purpose:** Unit testing framework.  
**Reason for adoption:** Simple, expressive, widely adopted in .NET.  
**Impact:** Foundation for all domain and application tests.

### FluentAssertions
**Purpose:** Human‑readable and expressive assertions.  
**Reason for adoption:** Improves clarity and determinism of tests.  
**Impact:** Makes tests easier to understand and maintain.

### NSubstitute (if used)
**Purpose:** Mocking framework for Application Layer tests.  
**Reason for adoption:** Minimal syntax; avoids boilerplate.  
**Impact:** Clean orchestration tests without domain behavior simulation.

### EF Core (Infrastructure Layer)
**Purpose:** Persistence implementation.  
**Reason for adoption:** Mature ORM; integrates well with Clean Architecture.  
**Impact:** Infrastructure only; never leaks into Domain or Application layers.

### DocFX Template (if customized)
**Purpose:** Standardize documentation layout.  
**Impact:** Ensures consistency across all generated docs.

---

## Architectural Decisions (ADRs)

### ADR‑001: Domain‑Driven Design + Clean Architecture
**Reason:** Complex domain with strong invariants; need for isolation and clarity.  
**Impact:** Defines project structure, boundaries, and testing strategy.

### ADR‑002: Domain Events as consistency mechanism
**Reason:** Clear communication of business facts; decoupled side effects.  
**Impact:** All aggregates emit domain events; Application Layer dispatches them.

### ADR‑003: Use Cases as Application Layer orchestration
**Reason:** Explicit orchestration; separation of concerns.  
**Impact:** Application tests validate interactions, not domain rules.

### ADR‑004: Mutation Testing required for Domain Layer
**Reason:** Guarantees invariants are protected by tests.  
**Impact:** Stryker.NET integrated into CI with minimum score threshold.

---

## Dependency Map

### Domain Layer
- No external dependencies  
- Only domain entities, value objects, events, exceptions  

### Application Layer
- Repositories  
- Unit of Work  
- Use Cases  
- Domain aggregates  

### Infrastructure Layer
- EF Core  
- Logging  
- Persistence  
- External integrations  

### Documentation
- DocFX  
- Markdown files  
- XML comments  

### Testing
- xUnit  
- FluentAssertions  
- Stryker.NET  
- NSubstitute (Application Layer only)

---

## Maintenance Rules
- Every new tool must be added to this document.  
- Every architectural decision must be recorded as an ADR.  
- No tool or dependency may be added without explicit justification.  
- This document must remain minimalistic, objective, and up‑to‑date.

