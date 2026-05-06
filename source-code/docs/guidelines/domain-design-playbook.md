# Domain Design Playbook (DDD + Clean Architecture)

## Language Rule
All documentation must be written in English.

---

## Purpose
This playbook defines the rules, boundaries, and modeling principles for designing domain models using Domain‑Driven Design (DDD) and Clean Architecture.  
Its goal is to ensure **consistency, correctness, clarity, and alignment with the ubiquitous language**, especially when domain models are generated or extended by AI.

---

## Principles
1. Domain first  
2. Behavior over data  
3. Invariants define boundaries  
4. Meaning over implementation  
5. Consistency over normalization  
6. Explicit boundaries and responsibilities  
7. Deterministic and unambiguous modeling  

---

## Core Domain Concepts

### Domain First Thinking
- Model based on business rules  
- Avoid database‑driven design  
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
- ❌ No cross‑aggregate coupling  
- ❌ No business logic outside domain  

### Size Rule
- Keep aggregates small and focused  
- Prefer consistency boundaries over normalization  

### Transaction Boundary
- One aggregate = one consistency boundary  
- Cross‑aggregate changes must be eventual consistency  

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
- Not contain cross‑aggregate logic  

Allowed:
- Internal state transitions  
- Validation within entity scope  
- Domain‑specific rules  

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
- Name  

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
- Return fully‑loaded aggregates  
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

## Anti‑Patterns

❌ Anemic Domain Model  
❌ Business logic in Application Layer  
❌ God Aggregates  
❌ Primitive Obsession  
❌ Cross‑Aggregate Coupling  
❌ Repositories returning IQueryable  
❌ Domain Events used as method calls  

---

## Quality Checklist

- [ ] Aggregate enforces all invariants  
- [ ] Business logic is inside domain  
- [ ] Events represent real business facts  
- [ ] No cross‑aggregate coupling  
- [ ] Value objects used correctly  
- [ ] Naming reflects business language  
- [ ] Repositories return aggregates only  
- [ ] Domain services used appropriately  
- [ ] Factories used for complex creation  
- [ ] Integration events follow Outbox Pattern  

---

## AI Usage Rules

When generating domain code, AI must:

### Always:
- Identify the Aggregate Root first  
- Enforce invariants inside aggregates only  
- Prefer domain events for side effects  
- Use value objects whenever possible  
- Keep domain behavior inside the domain  
- Follow the ubiquitous language  
- Follow all rules in this playbook  

### Never:
- Place business logic in application layer  
- Generate anemic models  
- Introduce cross‑aggregate coupling  
- Invent domain rules  
- Expose internal entities  
- Use IQueryable in repositories  

---

## Final Notes
The domain model is the heart of the system.  
It must be expressive, consistent, intentional, and aligned with the business language.  
This playbook ensures that all domain modeling follows the same principles and produces a coherent, maintainable, and evolvable domain.

