# Identity Module

## Overview

The Identity module is responsible for:

- Tenant management
- User identity lifecycle
- Multi-tenant authorization
- External identity binding (OIDC)

This module ensures that all operations are scoped within a tenant boundary.

---

## Core Concepts

### Tenant

Represents an isolated business boundary.

- Each tenant has its own users and permissions
- Identified by a unique name
- Controls access to the system

### Identity User

Represents a user authenticated via an external identity provider.

- Linked to an external provider (OIDC)
- Can belong to multiple tenants

### Membership

Defines the relationship between a user and a tenant.

- Contains role information
- Supports invitation-based onboarding

---

## Business Rules (Invariants)

- A tenant cannot have duplicate active invitations for the same email
- A membership can only be bound to a single identity user
- A user must belong to a tenant to access protected resources
- Deactivated entities must not be used in active flows

---

## Key Flows

### Tenant Invitation Flow

1. Tenant invites a user via email
2. Membership is created without IdentityUser
3. User authenticates via external provider
4. System binds IdentityUser to membership

---

## Domain Model

### Aggregates

- @Atlas.Identity.Domain.Entities.Tenant
- @Atlas.Identity.Domain.Entities.IdentityUser

### Entities

- @Atlas.Identity.Domain.Entities.TenantMembership

### Audit

- @Atlas.Identity.Domain.Entities.IdentityAuditLog

---

## API Reference

Detailed API documentation for domain classes:

👉 [View Identity Domain API](../../api/identity/)