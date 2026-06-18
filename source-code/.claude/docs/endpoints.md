# Endpoints (FastEndpoints)

## Rules

✅ Always inherit from `AtlasEndpoint<TReq, TRes>` — never `Endpoint<TReq, TRes>` directly
✅ Endpoint names use the HTTP resource name, not the domain command name
✅ Request and Response are **exclusive per endpoint** — never shared between endpoints
✅ Response is a `sealed record` with a static `From(Output)` factory method (commands) or `From(Dto)` (queries)
✅ Authorization: use string literal `"permission:"` prefix — not `HasPermissionAttribute.PolicyPrefix`
✅ OpenAPI: declare only the success response — error codes are added automatically by the transformer
✅ `Atlas.API` endpoints may orchestrate multiple module handlers when the frontend needs a unified contract
✅ Every new endpoint must add or update the corresponding Bruno request in `bruno/`
❌ Never use MVC controllers for new endpoints
❌ Never handle the error path manually — `AtlasEndpoint` base handles it via `result.IsSuccess`
❌ Never add `ProducesProblem(404/409/422...)` — `ProblemDetailsOperationTransformer` adds them globally
❌ Never reuse a Request or Response type across two endpoints — projections will diverge
❌ Never finish an endpoint change without updating the matching Bruno collection entry

## Naming

```
✅ CreateInvitationEndpoint   ← HTTP resource name
❌ InviteUserEndpoint        ← domain intent — wrong layer

✅ ListInvitationsEndpoint
✅ RevokeInvitationEndpoint  ← exceptional business verb is acceptable
```

## Folder Structure

```
src/{Module}/Atlas.{Module}.BffApi/Endpoints/{Resource}/{Operation}/
├── {Operation}Request.cs
├── {Operation}Response.cs    ← only if the endpoint returns a body
└── {Operation}Endpoint.cs
```

## Response Pattern

Response is the HTTP contract — it decouples the Application Output/DTO from what the client sees.
The static `From` method is the mapper passed to the result helper.

```csharp
// Commands: maps from handler Output
public sealed record CreateInvitationResponse(Guid InvitationId, string Email, DateTime ExpiresAt)
{
    public static CreateInvitationResponse From(InviteUserOutput output)
        => new(output.InvitationId, output.Email, output.ExpiresAt);
}

// Queries: maps from reader Dto (same pattern, different source)
public sealed record RoleResponse(Guid RoleId, string Name, IReadOnlyList<string> PermissionCodes)
{
    public static RoleResponse From(RoleDto dto)
        => new(dto.RoleId, dto.Name, dto.PermissionCodes);
}
```

## Endpoint Pattern

```csharp
public sealed class CreateInvitationEndpoint : AtlasEndpoint<CreateInvitationRequest, CreateInvitationResponse>
{
    public override void Configure()
    {
        Post("bff/v1/identity/invitations");
        Policies($"permission:{PermissionCatalog.Tenant.Invitations.Create}");
        Description(d => d.Produces<CreateInvitationResponse>(201));
    }

    public override async Task HandleAsync(CreateInvitationRequest req, CancellationToken ct)
    {
        var cmd    = new InviteUserCommand(req.Email, req.RoleId);
        var result = await _invoker.InvokeAsync(_handler, cmd, ct);
        await CreatedFromResultAsync(result, CreateInvitationResponse.From, ct);
    }
}
```

## Cross-Module Orchestration

`Atlas.API` is allowed to expose endpoints that unify multiple module-owned handlers behind one HTTP contract.

Preferred pattern:

```csharp
var target = ResolveTarget(req.ModuleId);
if (target is null)
{
    await Send.NotFoundAsync(ct);
    return;
}

var authorization = await authorizationService.AuthorizeAsync(
    User,
    policyName: $"permission:{target.Permission}");

if (!authorization.Succeeded)
{
    await Send.ForbiddenAsync(ct);
    return;
}

var result = await target.ExecuteAsync(command, ct);
await UpdatedFromResultAsync(
    result,
    output => Response.From(output, target.ModuleId, target.ModuleName),
    ct);
```

Use this pattern when:
- the frontend needs one endpoint for multiple modules
- the selected permission depends on request data such as `ModuleId`
- each module already owns its own handler

Keep the boundary clear:
- `Atlas.API` chooses the target
- module handler owns the business logic

## Result Helpers (AtlasEndpoint)

```csharp
await CreatedFromResultAsync(result, Response.From, ct);         // 201
await UpdatedFromResultAsync(result, Response.From, ct);         // 200
await UpdatedNoContentFromResultAsync(result, ct);               // 204
await DeletedFromResultAsync(result, ct);                        // 204
await OkFromResultAsync(result, ct);                             // 200 — no mapping needed
await OkFromResultAsync(result, Response.From, ct);              // 200 — with mapping
```

## Request vs Command

```csharp
// Request — HTTP contract, lives in BffApi. Only what the client sends.
public sealed record CreateInvitationRequest(string Email, Guid RoleId);

// Command — handler input, lives in Application. May have extra fields (e.g. TenantId from session).
public sealed record InviteUserCommand(string Email, Guid RoleId);
```
