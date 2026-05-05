# Domain Design Playbook (DDD + Clean Architecture)

## Index
- [Principles](#principles)
- [Core Domain Concepts](#core-domain-concepts)
- [Aggregate Design](#aggregate-design)
- [Aggregate Boundaries](#aggregate-boundaries)
- [Aggregate Invariants](#aggregate-invariants)
- [Entities](#entities)
- [Value Objects](#value-objects)
- [Domain Services](#domain-services)
- [Factories](#factories)
- [Repositories](#repositories)
- [Domain Events](#domain-events)
- [Integration Events & Outbox](#integration-events--outbox)
- [Use Cases](#use-cases)
- [Consistency Rules](#consistency-rules)
- [Event Storming](#event-storming)
- [Domain Evolution](#domain-evolution)
- [Anti-Patterns](#anti-patterns)
- [Quality Checklist](#quality-checklist)
- [AI Code Generation Rules](#ai-code-generation-rules)

---

## Principles

1. Domain first  
2. Behavior over data  
3. Invariants define boundaries  
4. Meaning over implementation  
5. Consistency over normalization  

---

## Core Domain Concepts

### Domain First Thinking
- Model based on business rules  
- Avoid database-driven design  
- Prioritize meaning and behavior  

### Ubiquitous Language
- Code must reflect business terminology  
- Consistent naming across aggregates, events, and use cases  

### Explicit Boundaries
- Each bounded context owns its model  
- No direct manipulation across contexts  
- Communication via events or contracts  

---

## Aggregate Design

### Responsibilities
- Enforce invariants  
- Control lifecycle of internal entities  
- Expose behavior, not state  

### Hard Rules
- ❌ No external mutation  
- ❌ No partial updates  
- ❌ No cross-aggregate coupling  
- ❌ No business logic outside domain  

### Size Rule
- Keep aggregates small and focused  
- Prefer consistency boundaries over normalization  

### Transaction Boundary
- One aggregate = one consistency boundary  
- Cross-aggregate changes must be eventual consistency  

---

## Aggregate Boundaries

Boundaries must be defined by invariants.

Guidelines:
- If two pieces of data must change together → same aggregate  
- If they change independently → separate aggregates  
- If enforcing invariants requires referencing another entity → include it  

---

## Aggregate Invariants

Invariants must always be true after any domain operation.

Examples:
- A Tenant cannot invite the same email twice unless previous invitation was used or expired  
- A User must always belong to exactly one Tenant  
- An Invitation cannot be used after expiration  

Only the Aggregate Root enforces invariants.

---

## Entities

Entities inside aggregates must:
- Represent domain concepts with identity  
- Be controlled only by the Aggregate Root  
- Not contain cross-aggregate logic  

Allowed:
- Internal state transitions  
- Validation within entity scope  
- Domain-specific rules  

Forbidden:
- Calling repositories  
- Calling external services  
- Handling persistence logic  

---

## Value Objects

Value Objects must:
- Be immutable  
- Represent a concept, not a table  
- Be replaceable, not mutated  
- Implement equality by value  
- Validate on creation  
- Never expose setters  

Examples:
- Email  
- Money  
- Role  
- Slug  

---

## Domain Services

Used when:
- A rule does not belong to any specific aggregate  
- Logic spans multiple aggregates  
- Behavior is not naturally owned by an entity or value object  

Characteristics:
- Stateless  
- Express domain operations  
- Depend only on domain abstractions  
- Cannot access infrastructure  

---

## Factories

Factories are used when creating an aggregate requires:
- Multiple steps  
- Complex invariants  
- Multiple entities or value objects  

Factories:
- Encapsulate creation logic  
- Ensure aggregates are always valid  
- Avoid leaking construction details  

---

## Repositories

Repositories represent collections of aggregates.

Rules:
- One repository per aggregate root  
- Return fully-loaded aggregates  
- Never return internal entities  
- Never expose IQueryable  
- Never contain business logic  
- Only application layer calls repositories  

---

## Domain Events

### Purpose
- Represent meaningful business facts  
- Describe what happened, not what to do  

### Rules
- Emitted only by Aggregate Root  
- Immutable  
- Named in past tense  
- Not used for internal control flow  
- Not treated as method calls  

### Usage
- Integration between bounded contexts  
- Side effects (email, logging, notifications)  
- Decoupling application logic  

---

## Integration Events & Outbox

Domain Events ≠ Integration Events.

### Domain Events
- Internal to bounded context  
- Part of domain model  
- Published before commit  

### Integration Events
- External communication  
- Published after commit  
- Must use Outbox Pattern  

### Outbox Pattern
- Store integration events in DB  
- Commit with same transaction  
- Background worker publishes to message bus  

---

## Use Cases

Use Cases must:
- Orchestrate aggregates only  
- Never contain business rules  
- Never enforce invariants  
- Never mutate domain state  

Responsibilities:
- Load aggregates  
- Call domain methods  
- Persist changes  
- Dispatch domain events  

Forbidden:
- Business decisions  
- Validation logic  
- Domain state mutations  

---

## Consistency Rules

### Strong Consistency
- Inside aggregate only  
- Must be enforced immediately  

### Eventual Consistency
- Between aggregates  
- Handled via domain events  

---

## Event Storming

Event Storming helps discover:
- Domain Events  
- Commands  
- Aggregates  
- Policies  
- Read models  

Benefits:
- Shared understanding  
- Clear boundaries  
- Discovery of invariants  
- Identification of missing concepts  

---

## Domain Evolution

Rules for evolving the domain model:
- Add new events instead of modifying existing ones  
- Add new methods instead of changing invariants  
- Avoid breaking changes to aggregates  
- Keep invariants explicit and documented  
- Use versioning for integration events  

---

## Anti-Patterns

- ❌ Anemic Domain Model  
- ❌ Business logic in Application Layer  
- ❌ God Aggregates  
- ❌ Primitive Obsession  
- ❌ Cross-Aggregate Coupling  
- ❌ Repositories returning IQueryable  
- ❌ Domain Events used as method calls  

---

## Quality Checklist

- [ ] Aggregate enforces all invariants  
- [ ] Business logic is inside domain  
- [ ] Events represent real business facts  
- [ ] No cross-aggregate coupling  
- [ ] Value objects used correctly  
- [ ] Naming reflects business language  
- [ ] Repositories return aggregates only  
- [ ] Domain services used appropriately  
- [ ] Factories used for complex creation  
- [ ] Integration events follow Outbox Pattern  

---

## AI Code Generation Rules

When generating domain code:
- Identify Aggregate Root first  
- Never place business logic in application layer  
- Prefer domain events for side effects  
- Use value objects when possible  
- Enforce invariants inside aggregates only  
- Avoid anemic models completely  
