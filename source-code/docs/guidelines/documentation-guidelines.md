# Documentation Guidelines

## Principles

1. Documentation must live close to the code
2. Comments explain intent, not implementation
3. Avoid duplication across code and markdown
4. Prefer clarity over completeness

---

## Documentation Layers

### Domain (Code - XML Comments)
- Explain business intent
- Define invariants
- Describe boundaries
- Follow DDD commenting guidelines

### Application (Code - XML Comments)
- Describe responsibilities
- Clarify orchestration logic
- Explicitly state what is NOT handled

### Modules (Markdown)
- High-level overview
- Business context
- Use cases

### Architecture (Markdown + Diagrams)
- System boundaries
- Integrations
- Design decisions

---

## Anti-patterns

- ❌ Explaining obvious code
- ❌ Repeating code in comments
- ❌ Writing generic descriptions
- ❌ Documenting only in README

---

## AI Usage Rules

When generating code:

- Always include XML documentation for:
  - Aggregate Roots
  - Value Objects
  - Use Cases

- Follow DDD commenting guidelines strictly
- Do NOT generate redundant or obvious comments