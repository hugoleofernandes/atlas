# Atlas Documentation

---

## 🚀 Start here

- [**Project Overview**](overview.md) — what Atlas is and how it's structured
- [DocFX Usage](guides/docfx-usage.md) — how this documentation is built

**New to the codebase?** Read [Overview](overview.md) → [Request Lifecycle](flows/request-lifecycle.md) → [User Onboarding](flows/user-onboarding.md). 30 minutes total, full mental model.

---

## 🔁 Flows — end-to-end with diagrams

How the system actually works, step by step.

- [**Request Lifecycle**](flows/request-lifecycle.md) — HTTP request → middleware → workflow → domain → response
- [**Outbox Integration**](flows/outbox-integration.md) — domain events → outbox → worker → other module
- [**User Onboarding**](flows/user-onboarding.md) — invitation → OIDC login → bootstrap → cross-module StaffMember creation
- [**Error Handling**](flows/error-handling.md) — where errors originate, how they reach the client, i18n

---

## 📦 Modules

- [Identity Module](modules/identity/index.md) — tenants, users, invitations
- [Staff Module](modules/staff/index.md) — staff members, integration event consumer

---

## 🏗️ Architecture

- [Building Blocks](architecture/building-blocks.md) — CQRS, pipelines, audit, validation
- [CQRS Pipeline](architecture/cqrs-pipeline.md) — command/query architecture
- [Outbox Worker Design](architecture/outbox-worker-design-architecture.md) — worker service internals
- [Technical Decisions & Tooling](architecture/technical-decisions-tooling.md) — why we chose what

---

## 📐 Guidelines

Coding rules and conventions — read before extending the system.

- [Domain Design Playbook](guidelines/domain-design-playbook.md)
- [DDD Commenting Guidelines](guidelines/DDD-commenting-guidelines.md)
- [Documentation Guidelines](guidelines/documentation-guidelines.md)
- [Programming Principles](guidelines/programming-principles-guide.md)
- [Unit Testing Guidelines](guidelines/unit-testing-guidelines.md)
- [Application Testing Guidelines](guidelines/application-testing-guidelines.md)
- [Guideline for Creating Guidelines](guidelines/guideline-for-creating-guidelines.md)

---

## 🔧 API Reference (auto-generated)

- [Identity API](api/identity/)
- [Staff API](api/staff/)
