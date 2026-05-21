# User Onboarding — End-to-End

The most important concrete flow in Atlas. Crosses both modules and exercises every architectural pattern.

**Scenario:** an admin invites a new person to a tenant. The person logs in via Entra ID. Atlas creates the User in Identity and reactively creates the StaffMember in Staff.

---

## The full picture

```mermaid
sequenceDiagram
    autonumber
    actor Admin
    actor User
    participant API as Atlas.API
    participant IDB as Identity DB
    participant Entra as Entra ID
    participant W as OutboxWorker
    participant SDB as Staff DB

    rect rgb(240, 245, 255)
    Note over Admin,IDB: Phase 1 — Invitation
    Admin->>API: POST /tenants/{name}/invitations<br/>{ email, role }
    API->>API: InviteUserWorkflow<br/>tenant.InviteUser(...)
    API->>IDB: COMMIT<br/>Invitation row<br/>UserInvitedDomainEvent → outbox
    API-->>Admin: 200 OK
    end

    rect rgb(245, 255, 240)
    Note over User,Entra: Phase 2 — Login (OIDC)
    User->>API: GET /auth/login?tenant=acme
    API-->>User: 302 → Entra authorize
    User->>Entra: authenticate
    Entra-->>User: 302 → callback?code=...
    User->>API: GET /signin-oidc?code=...
    API->>Entra: exchange code → tokens
    Entra-->>API: id_token (claims: oid, email, tenant_name)
    API-->>User: Set-Cookie (auth)
    end

    rect rgb(255, 250, 240)
    Note over User,IDB: Phase 3 — Bootstrap (first authenticated request)
    User->>API: GET /session/me<br/>(cookie attached)
    API->>API: TenantResolverMiddleware<br/>(tenant_name claim → TenantId)
    API->>API: UserBootstrapMiddleware<br/>(claim "bootstrap_completed" missing)
    API->>API: ResolveTenantAccessWorkflow<br/>tenant.ResolveAccess(oid, email)
    Note over API: Domain logic:<br/>· find Invitation by email<br/>· invitation.Use()<br/>· create User from invitation
    API->>IDB: COMMIT<br/>User row · Invitation.IsUsed=true<br/>UserCreatedFromInvitationDomainEvent → outbox
    API-->>User: 200 OK + new claims in cookie
    end

    rect rgb(255, 240, 245)
    Note over W,SDB: Phase 4 — Async cross-module integration
    W->>IDB: SELECT FOR UPDATE SKIP LOCKED
    IDB-->>W: outbox row (UserCreatedFromInvitationIntegrationEvent)
    W->>W: Dispatcher resolves handler<br/>(CreateStaffMemberIntegrationEventHandler<br/>in Staff module)
    W->>SDB: COMMIT<br/>StaffMember row<br/>(idempotent: skip if UserId already exists)
    W->>IDB: UPDATE outbox.processed_on = NOW()
    end
```

---

## What just happened, in plain English

1. **Invitation phase.** Admin sends an invitation. Identity persists an `Invitation` row. A domain event fires, but no integration event yet — Staff doesn't care about pending invitations.

2. **Login phase.** User completes the OIDC dance with Entra ID. Atlas just trusts the resulting cookie — no domain state changes yet. The user has a session, but Atlas hasn't yet looked up "who is this person in our domain?".

3. **Bootstrap phase.** First real request after login. `UserBootstrapMiddleware` notices the user hasn't been bootstrapped (no internal `bootstrap_completed` claim) and runs `ResolveTenantAccessWorkflow`. This is where the magic happens:
   - The Tenant aggregate is loaded with its Invitations and Users.
   - `tenant.ResolveAccess(oid, email)` finds the matching invitation, marks it used, creates the User.
   - Domain events are emitted. `IntegrationEventEnqueuer` maps `UserCreatedFromInvitationDomainEvent` → integration event → outbox row.
   - **Single transaction:** User created, Invitation used, outbox row written. All or nothing.
   - The middleware appends internal claims (TenantId, UserId, Role, `bootstrap_completed=true`) to the cookie. Future requests skip bootstrap.

4. **Integration phase.** `Atlas.OutboxWorker` (a separate process) polls Identity's outbox table. It finds the row, dispatches to the `CreateStaffMemberIntegrationEventHandler` in the Staff module, which creates a `StaffMember` linked by `UserId`. The handler is **idempotent** — if the StaffMember already exists, it skips.

---

## Why this design is robust

| Concern | How it's addressed |
|---|---|
| What if the user closes the browser between login and bootstrap? | Cookie persists. Next request hits bootstrap. No state lost. |
| What if Identity commits but the worker dies before creating StaffMember? | Outbox row stays Pending. Another worker (or restarted one) picks it up. |
| What if the worker dispatches but Staff DB is down? | Handler throws → retry. After max retries → dead-letter for manual review. |
| What if the user logs in twice during bootstrap? | `tenant.ResolveAccess()` finds the existing user on the second call and returns it. Idempotent at the domain level. |
| What if a different OID tries to log in with the same email? | `ResolveAccess` throws `UserAlreadyExistsException` — blocks impersonation. |
| What if the invitation expired? | `ResolveAccess` throws `InvitationExpiredException` → middleware returns 401 with localized message. |

---

## Files involved (for the curious)

| Phase | Key files |
|---|---|
| Invitation | `Atlas.Identity.Application/Tenants/Workflows/InviteUser/*` |
| Login | `Atlas.API/Security/Oidc/OidcMultiTenantConfigurator.cs` |
| Bootstrap | `Atlas.API/Security/Bootstrap/UserBootstrapMiddleware.cs` + `Atlas.Identity.Application/Tenants/Workflows/ResolveTenantAccess/*` |
| Integration | `Atlas.OutboxWorker/Processing/OutboxProcessor.cs` + `Atlas.Staff.Application/IntegrationEventHandlers/CreateStaffMemberIntegrationEventHandler.cs` |
