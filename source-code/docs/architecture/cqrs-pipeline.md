# CQRS Pipeline

## Purpose
Defines how commands and queries are executed in the application layer using MediatR and pipeline behaviors.
Makes the flow of validation, use case execution, transaction commit, and auditing explicit and deterministic.

---

## High-Level Flow

For a command:

1. Client sends a command to the system
2. MediatR receives the command
3. ValidationBehavior validates the command
4. Handler executes the command (usually delegating to a use case)
5. TransactionBehavior commits all Unit of Work instances
6. AuditBehavior records an audit entry
7. Response is returned to the client

For a query:

1. Client sends a query
2. MediatR receives the query
3. ValidationBehavior validates the query (if validators exist)
4. Handler executes the query
5. No transaction commit
6. No audit
7. Response is returned to the client

---

## Key Types and Locations

- ICommand<TResult>  
  /src/Atlas.BuildingBlocks/CQRS/Abstractions/ICommand.cs

- ICommandHandler<TCommand, TResult>  
  /src/Atlas.BuildingBlocks/CQRS/Abstractions/ICommandHandler.cs

- IQuery<TResult>  
  /src/Atlas.BuildingBlocks/CQRS/Abstractions/IQuery.cs

- IQueryHandler<TQuery, TResult>  
  /src/Atlas.BuildingBlocks/CQRS/Abstractions/IQueryHandler.cs

- ValidationBehavior<TRequest, TResponse>  
  /src/Atlas.BuildingBlocks/CQRS/Behaviors/ValidationBehavior.cs  
  (and/or /src/Atlas.BuildingBlocks/Validation/ValidationBehavior.cs)

- TransactionBehavior<TRequest, TResponse>  
  /src/Atlas.BuildingBlocks/CQRS/Behaviors/TransactionBehavior.cs

- AuditBehavior<TRequest, TResponse>  
  /src/Atlas.BuildingBlocks/Audit/AuditBehavior.cs

- IAuditStore  
  /src/Atlas.BuildingBlocks/Audit/IAuditStore.cs

- AuditEntry  
  /src/Atlas.BuildingBlocks/Audit/AuditEntry.cs

- ICurrentUser  
  /src/Atlas.BuildingBlocks/Audit/ICurrentUser.cs

- ICurrentTenant  
  /src/Atlas.BuildingBlocks/Audit/ICurrentTenant.cs

- IUnitOfWork  
  /src/Atlas.SharedKernel/Application/IUnitOfWork.cs (or equivalent)

---

## Behaviors

### ValidationBehavior

File:  
/src/Atlas.BuildingBlocks/CQRS/Behaviors/ValidationBehavior.cs

Responsibility:

- Run all FluentValidation.IValidator<TRequest> for the incoming request
- Stop the pipeline if validation fails
- Throw FluentValidation.ValidationException when there are validation errors

Effect:

- Commands/queries with invalid input never reach the handler/use case.
- Use cases can assume that input has already been validated.

---

### TransactionBehavior

File:  
/src/Atlas.BuildingBlocks/CQRS/Behaviors/TransactionBehavior.cs

Responsibility:

- Commit all registered IUnitOfWork instances
- Only for requests that implement ICommand<TResponse>
- Ensure persistence happens once, after successful handler execution

Effect:

- Use cases do not need to call SaveChangesAsync.
- Commits are centralized and consistent.
- Queries never commit.

---

### AuditBehavior

File:  
/src/Atlas.BuildingBlocks/Audit/AuditBehavior.cs

Responsibility:

- Create an AuditEntry for each command
- Capture:
  - Action (command type name)
  - EntityName (command type name, by default)
  - EntityId (optional, can be extended)
  - UserId (via ICurrentUser)
  - TenantId (via ICurrentTenant)
  - Changes (serialized request payload)

Effect:

- Every command execution generates an audit entry.
- Auditing is decoupled from domain and use cases.

---

## Request Types

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

## Example Execution: ResolveTenantAccess

### Command

File:  
/src/Atlas.Identity.Application/Tenants/UseCases/ResolveTenantAccess/ResolveTenantAccessCommand.cs

- Record with:
  - TenantName
  - ExternalOid
  - Email
- Implements ICommand<ResolveTenantAccessResult>

### Validator (suggested)

File:  
/src/Atlas.Identity.Application/Tenants/UseCases/ResolveTenantAccess/ResolveTenantAccessValidator.cs

- Ensures:
  - TenantName is not empty
  - ExternalOid is not empty
  - Email is not empty and is a valid email

### Use Case

File:  
/src/Atlas.Identity.Application/Tenants/UseCases/ResolveTenantAccess/ResolveTenantAccessUseCase.cs

Responsibilities:

- Load Tenant aggregate via ITenantRepository
- Call tenant.ResolveAccess(...)
- Map domain result to ResolveTenantAccessResult DTO
- Does not commit, does not audit, does not validate

### Handler (suggested)

File:  
/src/Atlas.Identity.Application/Tenants/UseCases/ResolveTenantAccess/ResolveTenantAccessHandler.cs

Responsibilities:

- Implement ICommandHandler<ResolveTenantAccessCommand, ResolveTenantAccessResult>
- Delegate to IResolveTenantAccessUseCase

---

## End-to-End Flow (Command)

1. API receives ResolveTenantAccessCommand
2. IMediator.Send(command) is called
3. ValidationBehavior executes ResolveTenantAccessValidator
4. ResolveTenantAccessHandler is invoked
5. Handler delegates to ResolveTenantAccessUseCase
6. Use case orchestrates domain (repository + aggregate)
7. TransactionBehavior calls SaveChangesAsync on all IUnitOfWork
8. AuditBehavior creates an AuditEntry with the command and context
9. ResolveTenantAccessResult is returned to the caller

---

## Testing Impact

Application tests:

- Verify:
  - Repository calls
  - Aggregate method invocation
  - DTO mapping
  - Propagation of domain exceptions
- Do not verify:
  - UoW commit (covered by TransactionBehavior)
  - Auditing (covered by AuditBehavior)
  - Validation rules (covered by ValidationBehavior)

Behavior tests:

- May exist for:
  - ValidationBehavior
  - TransactionBehavior
  - AuditBehavior

---

## Final Notes
The CQRS pipeline centralizes cross-cutting concerns (validation, transactions, auditing) and keeps use cases focused on orchestration only.
Commands and queries follow the same deterministic flow, making behavior predictable, testable, and easy to reason about.
