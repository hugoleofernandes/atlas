# Documentation Guidelines

## Language Rule
All documentation must be written in English.  
This includes Markdown files, architecture docs, module docs, diagrams, and all code comments.

---

## Purpose
This guideline defines how documentation must be written, structured, and maintained across the project.  
Its goal is to ensure **clarity, consistency, correctness, and alignment with the ubiquitous language**, especially when documentation is generated or extended by AI.

---

## Principles
1. Documentation must live close to the code  
2. Comments explain intent, not implementation  
3. Avoid duplication between code and markdown  
4. Prefer clarity over completeness  
5. Documentation must reflect the ubiquitous language  
6. Documentation must be deterministic and unambiguous  
7. Documentation must follow the same structure and tone across the project  

---

## What to Document

### Domain (Code – XML Comments)
- Business intent  
- Invariants  
- Aggregate boundaries  
- Domain events  
- Value object rules  
- Follow DDD Commenting Guidelines  

### Application (Code – XML Comments)
- Responsibilities  
- Orchestration logic  
- What the use case does NOT handle  
- Input/output contracts  

### Modules (Markdown)
- High‑level overview  
- Business context  
- Aggregates involved  
- Use cases  
- Domain events  
- Relationships between components  

### Architecture (Markdown + Diagrams)
- System boundaries  
- Bounded contexts  
- Integrations  
- Design decisions  
- Event flows  
- Recommended: Mermaid diagrams  

---

## What NOT to Document
- Implementation details  
- Obvious code  
- Repetition of domain rules already documented elsewhere  
- Temporary or framework‑specific behavior  
- Comments that restate the code  
- Diagrams that do not reflect real behavior  
- Information that belongs in another guideline  

---

## Boundaries of Documentation

### This guideline covers:
- How documentation must be structured  
- What must be documented  
- What must NOT be documented  
- Tone, style, and consistency rules  
- AI rules for generating documentation  

### This guideline does NOT cover:
- Domain modeling rules (see Domain Design Playbook)  
- Commenting rules (see DDD Commenting Guidelines)  
- Testing documentation (see Testing Guidelines)  
- Guideline creation rules (see Guideline for Creating Guidelines)  

---

## Tone and Style Requirements
- Use short sections  
- Use clear headers  
- Use lists instead of paragraphs  
- Avoid long explanations  
- Avoid academic language  
- Avoid ambiguity  
- Use separators (`---`) between major sections  
- Prefer imperative voice (“Describe”, “Document”, “Avoid”)  
- Keep documentation scannable and minimalistic  

---

## Diagrams

### When to Use
- To explain complex flows  
- To document aggregate behavior  
- To show domain event emission  
- To illustrate boundaries and invariants  
- To clarify architecture or module interactions  

### Recommended Formats
- Mermaid (preferred)  
- ASCII diagrams  
- Sequence diagrams for use cases  

---

## Examples

### Example: Domain XML Comment
<codeblock language="csharp">
/// <summary>
/// Represents a tenant that owns users and invitations.
/// Enforces invariants related to user creation and invitation lifecycle.
/// </summary>
</codeblock>

### Example: Module Documentation
<codeblock language="markdown">
# Identity Module

## Purpose
Manages tenants, users, and invitations.

## Aggregates
- Tenant
- User
- Invitation

## Domain Events
- UserInvited
- InvitationUsed
- UserCreatedFromInvitation
</codeblock>

---

## Checklist

Before finalizing documentation, verify:

- [ ] Documentation is written in English  
- [ ] Documentation reflects the ubiquitous language  
- [ ] No duplication with code or other markdown files  
- [ ] Intent is documented, not implementation  
- [ ] Boundaries and invariants are clear  
- [ ] Diagrams reflect real behavior  
- [ ] Tone and structure follow project standards  
- [ ] Documentation is scannable and minimalistic  
- [ ] AI rules were followed  

---

## Anti‑Patterns
❌ Explaining obvious code  
❌ Repeating domain rules already documented elsewhere  
❌ Generic comments  
❌ Documenting only in README  
❌ Diagrams that do not reflect real behavior  
❌ Overly verbose explanations  
❌ Mixing documentation concerns (domain, application, architecture)  

---

## AI Usage Rules

When generating documentation, AI must:

### Follow:
- Domain Design Playbook  
- DDD Commenting Guidelines  
- Documentation Guidelines  
- Guideline for Creating Guidelines  

### Always document:
- Aggregate Roots  
- Value Objects  
- Domain Events  
- Use Cases  

### Never:
- Generate redundant comments  
- Explain implementation details  
- Introduce new terminology without justification  

### Always:
- Focus on intent, invariants, and boundaries  
- Use consistent terminology  
- Use the required structure  
- Keep documentation short and objective  

---

## Final Notes
Documentation must:
- Be easy to read  
- Be easy to apply  
- Be easy to validate  
- Be consistent with the rest of the documentation  

Documentation is not a tutorial — it is a **source of truth**.
