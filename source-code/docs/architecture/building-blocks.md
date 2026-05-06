# Building Blocks

## Purpose
Defines the core abstractions and infrastructure patterns used across the solution:
CQRS, pipeline behaviors, Unit of Work, auditing, and validation.

---

## Structure

Suggested layout (adjust to your repo):

- /src/Atlas.BuildingBlocks/CQRS/Abstractions
- /src/Atlas.BuildingBlocks/CQRS/Behaviors
- /src/Atlas.BuildingBlocks/Audit
- /src/Atlas.BuildingBlocks/Validation (optional, if separated)
- /src/Atlas.SharedKernel/Application (Unit of Work, etc.)

---

## CQRS Abstractions

### Commands

File:  
/src/Atlas.BuildingBlocks/CQRS/Abstractions/ICommand.cs

Intent:

- Represent operations that modify state
- Always return a result (TResult)
- Are audited and committed

Handler:

/src/Atlas.BuildingBlocks/CQRS/Abstractions/ICommandHandler.cs

---

### Queries

File:  
/src/Atlas.BuildingBlocks/CQRS/Abstractions/IQuery.cs

Intent:

- Represent operations that do not modify state
- Never commit
- Are not audited

Handler:

/src/Atlas.BuildingBlocks/CQRS/Abstractions/IQueryHandler.cs

---

## Unit of Work

File:  
/src/Atlas.SharedKernel/Application/IUnitOfWork.cs (or similar)

Intent:

- Abstract persistence commit
- Allow multiple persistence contexts to be coordinated

Usage:

- Implemented by each persistence context (e.g., Identity, Staff, etc.)
- Registered in DI
- Consumed by TransactionBehavior

---

## Validation

### Validators

- Based on FluentValidation
- One validator per command/query that needs structural validation

Example responsibility:

- Ensure required fields are present
- Ensure basic format (e.g., email)

### ValidationBehavior

File:  
/src/Atlas.BuildingBlocks/CQRS/Behaviors/ValidationBehavior.cs  
or  
/src/Atlas.BuildingBlocks/Validation/ValidationBehavior.cs

Intent:

- Centralize validation
- Ensure handlers/use cases receive only valid input
- Throw ValidationException when validation fails

---

## Transactions

### TransactionBehavior

File:  
/src/Atlas.BuildingBlocks/CQRS/Behaviors/TransactionBehavior.cs

Intent:

- Centralize commits
- Ensure all IUnitOfWork instances are committed for commands
- Avoid scattered SaveChangesAsync calls in use cases
- Never commit for queries

---

## Auditing

### AuditEntry

File:  
/src/Atlas.BuildingBlocks/Audit/AuditEntry.cs

Intent:

- Represent a single audit record
- Capture:
  - Action
  - EntityName
  - EntityId (optional)
  - UserId
  - TenantId
  - Changes (serialized payload)
  - OccurredAtUtc

### IAuditStore

File:  
/src/Atlas.BuildingBlocks/Audit/IAuditStore.cs

Intent:

- Abstract persistence of audit entries
- Allow different storage implementations

### ICurrentUser / ICurrentTenant

Files:  
/src/Atlas.BuildingBlocks/Audit/ICurrentUser.cs  
/src/Atlas.BuildingBlocks/Audit/ICurrentTenant.cs

Intent:

- Provide current user and tenant context to the pipeline
- Decouple audit from HTTP or specific frameworks

### AuditBehavior

File:  
/src/Atlas.BuildingBlocks/Audit/AuditBehavior.cs

Intent:

- Record each executed command
- Associate command with current user and tenant
- Persist via IAuditStore
- Run only for commands

---

## Registration (DI + MediatR)

In a composition module (e.g., /src/Atlas.Api/Configuration/MediatRConfig.cs):

- Register MediatR with application assemblies
- Register pipeline behaviors:
  - ValidationBehavior
  - TransactionBehavior
  - AuditBehavior
- Register FluentValidation validators from application assemblies

Example responsibilities (conceitual):

- Ensure all commands/queries go through the same pipeline
- Ensure validators are discovered automatically
- Ensure behaviors are applied in the correct order

---

## Example End-to-End (Command)

1. API receives a command (e.g., ResolveTenantAccessCommand)
2. IMediator.Send(command) is called
3. ValidationBehavior executes the corresponding FluentValidation validator
4. The appropriate ICommandHandler is invoked
5. The handler delegates to the corresponding use case
6. The use case orchestrates domain behavior (repositories + aggregates)
7. TransactionBehavior commits all IUnitOfWork instances
8. AuditBehavior creates an AuditEntry with the command and context
9. The result is returned to the caller

---

## Principles

- Commands:
  - Modify state
  - Are validated, committed, and audited
- Queries:
  - Do not modify state
  - Are validated, but not committed or audited
- Use cases:
  - Orchestrate
  - Do not validate
  - Do not commit
  - Do not audit
- Domain:
  - Does not know about pipeline, UoW, auditing, or validation
  - Only enforces invariants and emits events

---

## Final Notes
These building blocks provide a consistent foundation for CQRS, validation, transactions, and auditing across all bounded contexts.
They keep cross-cutting concerns centralized and keep domain and application code focused on business behavior and orchestration.
