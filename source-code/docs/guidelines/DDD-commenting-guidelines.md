# DDD Commenting Guidelines

## Language Rule
All documentation must be written in English.  
This includes XML comments, domain descriptions, invariants, exceptions, and all Markdown files.

---

## Purpose
This guideline defines how comments must be written in a Domain‑Driven Design (DDD) codebase.  
Its goal is to ensure that comments document **business intent**, **invariants**, and **domain meaning**, not technical implementation.

---

## Principles
1. Document intent, not implementation  
2. Comments must reflect the ubiquitous language  
3. Invariants must be explicit and visible  
4. Comments must clarify boundaries and responsibilities  
5. Comments must be deterministic and unambiguous  
6. Avoid duplication between comments and domain rules  
7. Comments must help understand the domain, not the code mechanics  

---

## What to Document

### Aggregate Root
Each Aggregate Root must include:

#### Purpose
- What the aggregate represents in the domain  
- Why it exists  

#### Invariants
- Business rules that must **never** be violated  
- Must be explicitly listed  

#### Boundaries
- What the aggregate controls  
- What it explicitly does **not** control  

#### Design Decisions
- Non‑obvious modeling choices  
- Why certain entities or value objects belong to this aggregate  

---

### Entity (Inside Aggregate)
Document:
- The role of the entity within the aggregate  
- Lifecycle ownership (always the Aggregate Root)  
- Local invariants (if any)  

Do NOT:
- Repeat invariants already documented in the Aggregate Root  

---

### Value Object
Document:
- Domain meaning  
- Immutability guarantee  
- Validation rules  
- Why it is a Value Object instead of an Entity  

---

### Domain Events
Document:
- The business fact represented by the event  
- Why this event exists  
- When it should be emitted  
- Which invariants it communicates  

---

### Domain Exceptions
Document:
- Which invariant was violated  
- When the exception should be thrown  
- How it relates to the Aggregate Root  

---

### Use Cases (Application Layer)
Document:

#### Responsibilities
- What the use case orchestrates  
- Which aggregates it loads  
- Which domain behaviors it triggers  

#### Boundaries
Explicitly state what the use case does **not** handle:
- No business rules  
- No domain state mutation  
- No invariant enforcement  

---

## What NOT to Document
- Implementation details  
- Trivial getters/setters  
- Obvious operations  
- Repetition of domain rules already documented elsewhere  
- Technical explanations unrelated to domain meaning  
- Comments that restate the code  

---

## Method‑Level Comments
Use XML comments only when:
- The method represents **domain behavior**  
- The intent is not obvious from the name  
- There are important invariants or side effects  

Avoid:
- Commenting trivial methods  
- Describing implementation details  
- Explaining obvious behavior  

---

## Examples

### Example: Domain Behavior Comment
<codeblock language="csharp">
/// <summary>
/// Resolves access for a user within the tenant.
///
/// Invariants:
/// - Tenant must be active.
/// - User must exist or be created from a valid invitation.
/// - No two users may share the same email.
///
/// Emits:
/// - InvitationUsedDomainEvent
/// - UserCreatedFromInvitationDomainEvent
/// - UserAccessResolvedDomainEvent
/// </summary>
</codeblock>

---

## Checklist

Before finalizing comments, verify:

- [ ] Comments document intent, not implementation  
- [ ] Invariants are explicit and correct  
- [ ] Boundaries are clearly stated  
- [ ] Comments reflect the ubiquitous language  
- [ ] No duplication with other documentation  
- [ ] No trivial or redundant comments  
- [ ] Domain meaning is clear  
- [ ] Comments are deterministic and unambiguous  

---

## Anti‑Patterns
❌ Explaining obvious code  
❌ Repeating domain rules already documented elsewhere  
❌ Commenting trivial getters/setters  
❌ Describing implementation details  
❌ Using comments to justify bad design  
❌ Mixing domain and technical explanations  
❌ Writing comments that contradict domain rules  

---

## AI Usage Rules

When generating comments, AI must:

### Follow:
- Domain Design Playbook  
- DDD Commenting Guidelines  
- Documentation Guidelines  
- Guideline for Creating Guidelines  

### Always:
- Document intent, invariants, and boundaries  
- Use the ubiquitous language  
- Keep comments short and objective  
- Ensure comments are domain‑focused  
- Ensure comments are deterministic and unambiguous  

### Never:
- Generate redundant comments  
- Explain implementation details  
- Invent domain rules  
- Introduce terminology not present in the domain  

---

## Final Notes
Comments must:
- Clarify domain meaning  
- Make invariants explicit  
- Strengthen understanding of the model  
- Be consistent with all other documentation  

Comments are not for explaining code — they are for explaining the **domain**.
