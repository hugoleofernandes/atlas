# How to Use This Documentation (DocFX Guide)

## What is DocFX in this project?

DocFX is used to automatically generate API documentation from code.

It reads:
- C# classes
- Methods
- Entities
- DTOs

And generates:
- HTML reference pages
- Navigation (TOC)
- Cross-linking between types

---

## What is generated automatically?

Inside `/api`:

- Domain entities
- Application services
- Infrastructure components

You DO NOT edit these files manually.

---

## What is written manually?

Inside `/modules`:

- Business explanations
- Domain rules
- Architecture decisions

---

## Guidelines

- Never edit `/api`
- Always document domain rules in `/modules`
- Keep DDD rules inside `/guideline`

---

## How to extend documentation

1. Add new module → `/modules/new-module`
2. Add new project → update `docfx.json`
3. Run: