# Cross-Module Data Access

## Rules

✅ Cross-module **read** → orchestrate in `Atlas.API`: call each module's query handler, join the DTOs in C#
✅ Cross-module **write** → integration event via Outbox (atomic guarantee — event written in the same transaction as the aggregate)
✅ Composition lives **only** in `Atlas.API` — each module exposes its own query handlers
✅ Always batch — `GetXByIds(Guid[])` with `WHERE id = ANY(@Ids)`, never N+1
✅ Cross-module references are always **by Id (Guid)** — never a project reference
❌ Never JOIN across schemas in a module-owned reader — readers touch only their own schema
❌ Never replicate data between modules as a general policy — eventual consistency is a needless cost in a modular monolith
❌ Never compose cross-module reads inside a module — composition belongs to `Atlas.API`

## Why — Modular Monolith First

Atlas is a **modular monolith**: one process, one PostgreSQL instance, one schema per module. The architecture keeps modules decoupled so they *can* be split into services later — but until that day, we take the monolith's advantages instead of paying the distributed-systems tax early.

Replicating a Party's name into the Staff schema would introduce **eventual consistency inside a single process with a single database** — the most expensive cost of distributed systems (stale windows, reconciliation, backfill) applied exactly where it is not needed. We orchestrate instead: compose on demand, always fresh, single source of truth, zero sync code.

## Read — Orchestrate in Atlas.API

A cross-module read composes results from independent query handlers. Each reader stays inside its own schema; the join happens in memory in the orchestrating endpoint (projection, not business logic — allowed by `endpoints.md`).

```csharp
// Atlas.API/Endpoints/Staff/ListStaffWithNames/ListStaffWithNamesEndpoint.cs
var staff   = await _invoker.InvokeAsync(_listStaff, new ListStaffQuery(...), ct);
var partyIds = staff.Value.Select(s => s.PartyId).ToArray();

// batched — one query for all ids, never N+1
var parties = await _invoker.InvokeAsync(_getParties, new GetPartiesByIdsQuery(partyIds), ct);
var byId    = parties.Value.ToDictionary(p => p.PartyId);

var response = staff.Value.Select(s => StaffRow.From(s, byId[s.PartyId])).ToList();
```

The module-owned reader uses the array pattern already established for 1:N reads:

```sql
SELECT id, display_name AS DisplayName, tax_number AS TaxNumber
FROM atlas_partners.parties
WHERE id = ANY(@Ids)
```

## Write — Integration Event via Outbox

A cross-module write (e.g. registering a `Person` and its `StaffMember`) spans two `DbContext`s — two transactions. Direct orchestration would leave the second write unguaranteed if it fails after the first commits. The Outbox closes this: the integration event is written in the same transaction as the source aggregate, so if the source committed, the downstream handler *will* run (eventually, with retry). See `domain-events.md`.

This is the **only** place a cross-module event is justified. Reads never use events.

## The Decoupling Seam

Orchestration does not lock you into the monolith. `IHandlerInvoker` abstracts handler invocation: today it resolves in-process; if a module is later extracted into its own service, only that module's query-handler **transport** changes (in-process → HTTP/gRPC). The orchestrating endpoint in `Atlas.API` does not change.

```
Atlas.API orchestrator ──IHandlerInvoker──▶ GetPartiesByIds
                                              ▲
                          the seam: only this transport flips when decoupling
```

## Surgical Exception

If profiling later shows a specific hot query (e.g. a dashboard joining thousands of rows with a Party field, called per second) where repeated composition hurts, add **surgical** replication for that one projection — never as a blanket policy. YAGNI until the profiler complains.

## Party as Stable Core

The `Partners` module's `Party` (Person / Organization) is the **stable identity core** and holds **zero outbound references**. Role-bearing aggregates own the link: `StaffMember.PartyId`, `Customer.PartyId`, `Supplier.PartyId`, `User.PartyId`. Adding a new role type never touches `Party` (open/closed), and the same person can be staff, customer and user at once — three rows pointing at one `PartyId`.
