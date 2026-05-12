# Outbox Worker — Objective & Scope

## Purpose
Define the responsibilities, boundaries, and execution model of the Outbox Worker responsible for processing integration events stored in the `OutboxMessage` table.  
The goal is to ensure **reliable, deterministic, and isolated event dispatch**, following the Outbox Pattern and the architectural standards of the project.

---

## Responsibilities
The Outbox Worker must:

- Retrieve pending `OutboxMessage` records eligible for processing  
- Apply optimistic locking to avoid concurrent processing  
- Deserialize the event payload into the correct integration event type  
- Resolve and execute the corresponding `IIntegrationEventHandler<T>`  
- Update message state (`Processed`, `Failed`, `DeadLettered`)  
- Enforce retry rules and dead-lettering  
- Ensure idempotent and deterministic execution  
- Run independently from the Application Layer and Domain Layer  
- Provide observability (logging, metrics, tracing)

---

## Non‑Responsibilities
The Outbox Worker must **not**:

- Execute domain logic  
- Execute application use cases  
- Access domain aggregates  
- Perform business decisions  
- Validate domain invariants  
- Commit Unit of Work for domain operations  
- Handle HTTP, controllers, or external API orchestration  
- Perform cross‑aggregate coordination  
- Depend on Application Layer abstractions  
- Emit domain events  

---

## Processing Flow (High‑Level)

1. Fetch unprocessed and unlocked messages  
2. Attempt optimistic lock (`TryLock`)  
3. Deserialize payload into integration event  
4. Resolve handler based on `Type`  
5. Execute handler  
6. Update message state  
7. Persist changes  
8. Release lock  

---

## Message Eligibility Rules
A message is eligible for processing when:

- `ProcessedOn` is null  
- `DeadLetteredOn` is null  
- `LockedUntil` is null or expired  
- `RetryCount` < `MaxRetries`  
- `CanBeProcessed(maxRetries)` returns true  

---

## Failure Handling

- On handler exception → increment retry count  
- If retries exceeded → mark as dead‑lettered  
- Always clear lock after processing attempt  
- Store error message for diagnostics  

---

## Concurrency Model

- Multiple worker instances may run simultaneously  
- Locking ensures only one worker processes a message  
- Lock duration is short and configurable  
- Parallelism is configurable  

---

## Handler Resolution

- Event type is resolved via `OutboxMessage.Type`  
- Payload is deserialized into the corresponding event class  
- Handler is resolved via DI container  
- Handler implements `IIntegrationEventHandler<TEvent>`  

---

## Observability Requirements

The worker must emit:

- Logs for processing lifecycle  
- Metrics for processed/failed/dead-lettered messages  
- Traces for each message (OpenTelemetry)  
- CorrelationId propagation  

---

## Deployment Model

- HostedService (BackgroundService)  
- Runs inside Infrastructure layer  
- Independent from API lifecycle  
- Configurable interval and parallelism  

---

## Extensibility Rules

Adding a new integration event requires:

1. Create the IntegrationEvent  
2. Create the IntegrationEventHandler  
3. Register handler in DI  
4. Ensure Application Layer publishes the corresponding OutboxMessage  

No changes to the Worker are required.

---

## Out of Scope

- Eventual consistency policies  
- Saga orchestration  
- Message broker publishing logic  
- Cleanup/archiving of old messages  
- Manual reprocessing (separate module)  

---

## Quality Requirements

The worker must be:

- Deterministic  
- Idempotent  
- Isolated  
- Testable  
- Observable  
- Configurable  
- Resilient to failures  
- Free of coupling with Domain/Application  

---

# Fase 0 concluída.
